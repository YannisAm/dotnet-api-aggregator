using AgileActors.API.Clients;
using AgileActors.API.Interfaces;
using AgileActors.API.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using Polly;
using System.Reflection;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<WeatherClient>(client =>
{
    client.BaseAddress = new Uri("https://api.openweathermap.org/");
    client.DefaultRequestHeaders.Add("User-Agent", "AgileActors.API");
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500)));

builder.Services.AddHttpClient<NewsClient>(client =>
{
    client.BaseAddress = new Uri("https://newsapi.org/v2/");
    client.DefaultRequestHeaders.Add("User-Agent", "AgileActors.API");
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500)));

builder.Services.AddHttpClient<ExchangeRatesClient>(client =>
{
    client.BaseAddress = new Uri("https://v6.exchangerate-api.com/v6/");
    client.DefaultRequestHeaders.Add("User-Agent", "AgileActors.API");
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, _ => TimeSpan.FromMilliseconds(500)));

builder.Services.AddScoped<IAggregationService, AggregationService>();
builder.Services.AddTransient<IApiClientInterface, NewsClient>(sp => sp.GetRequiredService<NewsClient>());
builder.Services.AddTransient<IApiClientInterface, WeatherClient>(sp => sp.GetRequiredService<WeatherClient>());
builder.Services.AddTransient<IApiClientInterface, ExchangeRatesClient>(sp => sp.GetRequiredService<ExchangeRatesClient>());
builder.Services.AddSingleton<IStatisticsService, StatisticsService>();

builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Aggregation Service",
        Version = "v1",
        Description = "An API that aggregates Weather, News and Exchange Rates data."
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Aggregation v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();