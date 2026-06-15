using System.ClientModel;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.OpenApi.Models;
using OpenAI;
using Sanitizer;
using Sanitizer.Api.Auth;
using Sanitizer.Api.Middleware;
using Sanitizer.Api.Services;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Strategies;
using Sanitizer.Api.Swagger;
using Sanitizer.Components;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();

// Swagger: два документа — внутренний и публичный
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.ExampleFilters();

    c.SwaggerDoc("internal", new OpenApiInfo { Title = "Sanitizer Internal API" });
    c.SwaggerDoc("public", new OpenApiInfo { Title = "Sanitizer Public API" });

    // X-Auth-Token для внутреннего API
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Auth-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Внутренний API-ключ в формате GUID"
    });

    // Bearer для внешнего v1 API
    c.AddSecurityDefinition("ApiToken", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "OpenAI API Key (вставьте чистый токен sk-...)"
    });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var isPublic = apiDesc.RelativePath?
            .StartsWith("proxy/", StringComparison.OrdinalIgnoreCase) ?? false;
        
        if (docName == "public") return isPublic;
        if (docName == "internal") return !isPublic;
        return false;
    });

    c.OperationFilter<SwaggerSecurityOperationFilter>();
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<CreateConversationRequestExample>();


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

builder.Services.AddScoped<IProfileStorage, EfProfileStorage>();
builder.Services.AddScoped<IApiKeyStorage, EfApiKeyStorage>();
builder.Services.AddScoped<IChatStorage, EfChatStorage>();
builder.Services.AddScoped<IMessageStorage, EfMessageStorage>();
builder.Services.AddScoped<IUsersStorage, EfUsersStorage>();

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
builder.Services.AddScoped<UsersService>();
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHttpClient("OpenAI", client =>
{
    client.BaseAddress = new Uri("https://api.openai.com/");
    client.Timeout = TimeSpan.FromSeconds(100);
});

var config = builder
                 .Configuration
                 .GetSection("OpenAi")
                 .Get<OpenAiConfig>() ?? throw new Exception("OpenAI configuration not found");

var proxyConfig = builder
                      .Configuration
                      .GetSection("Proxy")
                      .Get<ProxyConfig>();

builder.Services.AddSingleton(OpenAiChatHelper.CreateChatClient(config, proxyConfig));

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

app.MapWhen(context => 
{
    var isApiPath = context.Request.Path.StartsWithSegments("/api");
    var isLoginPath = context.Request.Path.StartsWithSegments("/api/login");
    return isApiPath && !isLoginPath;
}, appBuilder =>
{
    appBuilder.UseMiddleware<ApiKeyAuthMiddleware>();
    appBuilder.UseRouting();
    appBuilder.UseEndpoints(endpoints => endpoints.MapControllers());
});

app.UseRateLimiter();
app.MapControllers();
app.MapReverseProxy();

await app.AddDefaultUsers();
await app.AddDefaultProfiles();

app.Run();