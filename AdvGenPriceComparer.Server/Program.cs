using AdvGenPriceComparer.Server.Data;
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

// Add database context
builder.Services.AddDbContext<PriceDataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=AdvGenPriceComparer.db"));

// Add application services
builder.Services.AddScoped<IPriceDataService, PriceDataService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

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

// Add API Key authentication middleware
app.UseMiddleware<ApiKeyMiddleware>();

// Add rate limiting middleware
app.UseMiddleware<RateLimitMiddleware>();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PriceDataContext>();
    context.Database.EnsureCreated();
}

app.Run();
