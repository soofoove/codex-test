using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<WeatherForecastDbContext>(options =>
    options.UseInMemoryDatabase("WeatherDb"));

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherForecastDbContext>();
    if (!db.Forecasts.Any())
    {
        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToArray();
        db.Forecasts.AddRange(forecast);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async (WeatherForecastDbContext db) =>
    await db.Forecasts.ToListAsync())
    .WithName("GetWeatherForecast");

app.MapPost("/weatherforecast", async (WeatherForecast forecast, WeatherForecastDbContext db) =>
    {
        db.Forecasts.Add(forecast);
        await db.SaveChangesAsync();
        return Results.Created($"/weatherforecast/{forecast.Id}", forecast);
    })
    .WithName("CreateWeatherForecast");

app.Run();

class WeatherForecast
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class WeatherForecastDbContext : DbContext
{
    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();
}
