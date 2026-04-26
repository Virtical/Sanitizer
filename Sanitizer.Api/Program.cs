using Microsoft.EntityFrameworkCore;
using Sanitizer.Api.Data;
using Sanitizer.Api.Middleware;
using Sanitizer.Api.Services;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Strategies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Sanitizer API", Version = "v1" });
    c.AddSecurityDefinition("ApiKey", new()
    {
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name        = "x-auth-token",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "API ключ (или AdminKey из конфига)"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "ApiKey" } },
            []
        }
    });
});

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:5173"];

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddDbContext<SanitizerDbContext>(opt =>
    opt.UseInMemoryDatabase("SanitizerDb"));

builder.Services.AddHttpClient<LlmProxyService>();

builder.Services.AddScoped<IProfileStorage, EfProfileStorage>();
builder.Services.AddScoped<IApiKeyStorage,  EfApiKeyStorage>();

builder.Services.AddSingleton<TokenStore>();
builder.Services.AddSingleton<DetectorRegistry>();
builder.Services.AddSingleton<StrategyFactory>();
builder.Services.AddSingleton<DesanitizerService>();
builder.Services.AddScoped<SanitizerService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<ApiKeyService>();

var llmProvider = builder.Configuration["Llm:Provider"]?.ToLowerInvariant() ?? "stub";
if (llmProvider == "openai")
    builder.Services.AddScoped<ILlmClient, OpenAiLlmClient>();
else
    builder.Services.AddSingleton<ILlmClient, StubLlmClient>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { Status = "healthy", Timestamp = DateTime.UtcNow }));

app.UseCors();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.MapControllers();

app.Run();
