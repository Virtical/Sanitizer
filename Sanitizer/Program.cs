using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using Sanitizer;
using Sanitizer.Api.Services;
using Sanitizer.Api.Storage;
using Sanitizer.Api.Storage.Data;
using Sanitizer.Api.Strategies;
using Sanitizer.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.EnableAnnotations());

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
builder.Services.AddScoped<IChatStorage, EfChatStorage>();
builder.Services.AddScoped<IMessageStorage, EfMessageStorage>();

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
    builder.Services.AddSingleton(new ChatClient(builder.Configuration["Llm:Model"], builder.Configuration["Llm:ApiKey"]));
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
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();

await app.AddDefaultProfiles();

app.Run();
