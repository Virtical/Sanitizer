using Microsoft.AspNetCore.RateLimiting;
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

    c.SwaggerDoc("internal", new() { Title = "Sanitizer Internal API", Version = "internal" });
    c.SwaggerDoc("v1", new() { Title = "Sanitizer Public API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "Публичный API-ключ в формате GUID"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
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
        return docName == "v1" ? isPublic : !isPublic;
    });
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173"];

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

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
builder.Services.AddSingleton<DetectorRegistry>();
builder.Services.AddSingleton<StrategyFactory>();
builder.Services.AddSingleton<DesanitizerService>();
builder.Services.AddScoped<SanitizerService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ChatHistoryService>();

var llmProvider = builder.Configuration["Llm:Provider"]?.ToLowerInvariant() ?? "stub";
if (llmProvider == "openai")
{
    builder.Services.AddSingleton(new ChatClient(
        builder.Configuration["Llm:Model"],
        builder.Configuration["Llm:ApiKey"]));
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
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Public API v1");
    c.SwaggerEndpoint("/swagger/internal/swagger.json", "Internal API");
});

app.UseCors();

// Middleware аутентификации публичного API (только для /api/public/*)
app.UseMiddleware<ApiKeyAuthMiddleware>();

app.UseRateLimiter();
app.MapControllers();

await app.AddDefaultProfiles();

app.Run();
