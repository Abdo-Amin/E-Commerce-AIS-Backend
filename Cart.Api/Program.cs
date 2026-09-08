using Cart.Api.Middleware;
using Cart.Api.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6379";
    var options = ConfigurationOptions.Parse(connectionString);

    options.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(options);
});

builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddHttpClient<IProductCatalogClient, ProductCatalogClient>(client =>
{
    var productApiUrl = builder.Configuration["Services:ProductApi"]
        ?? "http://localhost:5290";

    client.BaseAddress = new Uri(productApiUrl);
});

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "KPM API v1");
});

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }
