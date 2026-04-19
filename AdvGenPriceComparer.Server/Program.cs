using AdvGenPriceComparer.Server.Data;
using AdvGenPriceComparer.Server.Hubs;
using AdvGenPriceComparer.Server.Middleware;
using AdvGenPriceComparer.Server.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "AdvGen Price Comparer API",
        Version = "v1",
        Description = "API for sharing grocery price data between AdvGen Price Comparer clients"
    });
    
    // Add API Key authentication
    c.AddSecurityDefinition("ApiKey", new()
    {
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-Key",
        Description = "API Key authentication"
    });
});

// Add SignalR for real-time updates
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
    options.StreamBufferCapacity = 50;
});

// Add CORS for SignalR WebSocket support
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRPolicy", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Secure fallback
            policy.WithOrigins("https://localhost:none")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Add database context (allow override in testing environment)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<PriceDataContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=AdvGenPriceComparer.db"));
}

// Add application services
builder.Services.AddScoped<IPriceDataService, PriceDataService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// Add SignalR notification service
builder.Services.AddSingleton<INotificationService, SignalRNotificationService>();

// Add rate limiting
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IRateLimitService, RateLimitService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add CORS middleware before SignalR
app.UseCors("SignalRPolicy");

// Add API Key authentication middleware
app.UseMiddleware<ApiKeyMiddleware>();

// Add rate limiting middleware
app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<PriceUpdateHub>("/hubs/price-updates");

// Ensure database is created (skip during integration tests)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<PriceDataContext>();
        context.Database.EnsureCreated();
    }
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
