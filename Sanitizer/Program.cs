using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Sanitizer;
using Sanitizer.Api.Auth;
using Sanitizer.Api.Services;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Strategies;
using Sanitizer.Components;
using System.Threading.RateLimiting;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi.Models;
using OpenAI;
using Sanitizer.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Swagger: два документа — внутренний и публичный
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();

    c.SwaggerDoc("internal", new OpenApiInfo { Title = "Sanitizer Internal API" });
    c.SwaggerDoc("public", new OpenApiInfo { Title = "Sanitizer Public API" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Auth-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Публичный API-ключ в формате GUID"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });

    // Разделяем контроллеры по документам
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var isPublic = apiDesc.RelativePath?
            .StartsWith("api/public/", StringComparison.OrdinalIgnoreCase) ?? false;
        return docName == "public" ? isPublic : !isPublic;
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   // любые источники
            .AllowAnyMethod()   // любые HTTP методы (GET, POST, PUT, DELETE и т.д.)
            .AllowAnyHeader();  // любые заголовки
    });
});

builder.Services.AddDbContext<SanitizerDbContext>(opt =>
    opt.UseInMemoryDatabase("SanitizerDb"));

// Rate limiting для публичного API: 60 запросов/мин на токен
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("PublicApi", context =>
    {
        var apiKeyId = context.Items.TryGetValue("ApiKeyId", out var key)
            ? key?.ToString() ?? "anonymous"
            : "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            apiKeyId,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 60,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddHttpClient<LlmProxyService>();

builder.Services.AddScoped<IProfileStorage, EfProfileStorage>();
builder.Services.AddScoped<IApiKeyStorage, EfApiKeyStorage>();
builder.Services.AddScoped<IChatStorage, EfChatStorage>();
builder.Services.AddScoped<IMessageStorage, EfMessageStorage>();

// Контекст текущего API-ключа (scoped — живёт в рамках одного запроса)
builder.Services.AddScoped<CurrentApiKeyContext>();
builder.Services.AddScoped<ICurrentApiKeyContext>(sp =>
    sp.GetRequiredService<CurrentApiKeyContext>());

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddSingleton<DetectorRegistry>();
builder.Services.AddSingleton<StrategyFactory>();
builder.Services.AddSingleton<DesanitizerService>();
builder.Services.AddScoped<SanitizerService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ChatHistoryService>();

var llmProvider = builder.Configuration["Llm:Provider"]?.ToLowerInvariant() ?? "stub";
if (llmProvider == "openai")
{
    builder.Services.AddSingleton<IChatClient>(_ 
        => new OpenAIClient(builder.Configuration["Llm:ApiKey"])
            .GetChatClient(builder.Configuration["Llm:Model"])
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build()
        );
}
else
{
    builder.Services.AddSingleton<ILlmClient, StubLlmClient>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/public/swagger.json", "Public API");
    c.SwaggerEndpoint("/swagger/internal/swagger.json", "Internal API");
});

app.UseCors("AllowAll");

app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyAuthMiddleware>();
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(endpoints => endpoints.MapControllers());
});

app.UseRateLimiter();
app.MapControllers();

await app.AddDefaultProfiles();

app.Run();