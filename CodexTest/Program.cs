using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddDbContext<WeatherForecastDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHttpClient<WeatherService>();
builder.Services.AddHttpClient<CurrencyService>();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherForecastDbContext>();
    db.Database.EnsureCreated();
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

app.MapPatch("/weatherforecast/{id}", async (int id, JsonPatchDocument<WeatherForecast> patch, WeatherForecastDbContext db) =>
    {
        var forecast = await db.Forecasts.FindAsync(id);
        if (forecast is null)
        {
            return Results.NotFound();
        }
        patch.ApplyTo(forecast);
        await db.SaveChangesAsync();
        return Results.NoContent();
    })
    .WithName("PatchWeatherForecast");

app.MapDelete("/weatherforecast/{id}", async (int id, WeatherForecastDbContext db) =>
    {
        var forecast = await db.Forecasts.FindAsync(id);
        if (forecast is null)
        {
            return Results.NotFound();
        }
        db.Forecasts.Remove(forecast);
        await db.SaveChangesAsync();
        return Results.NoContent();
    })
    .WithName("DeleteWeatherForecast");

app.MapGet("/rates", async (CurrencyService currency, WeatherService weather, WeatherForecastDbContext db, IConfiguration config) =>
    {
        var temp = await weather.GetKyivTemperatureAsync() ?? 0.0;
        var thresholdStr = config["TEMP_THRESHOLD"];
        var threshold = double.TryParse(thresholdStr, out var t) ? t : 21.0;
        var rates = await currency.GetRatesAsync();
        foreach (var kv in rates)
        {
            db.Rates.Add(new CurrencyRate { Currency = kv.Key, Rate = (decimal)kv.Value, RetrievedAt = DateTime.UtcNow });
        }
        await db.SaveChangesAsync();
        if (temp < threshold)
        {
            rates = rates.ToDictionary(k => k.Key, v => v.Value * temp);
        }
        return Results.Ok(new { Temperature = temp, Rates = rates });
    })
    .WithName("GetRates");

app.Run();

class WeatherForecast
{
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

class CurrencyRate
{
    public int Id { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime RetrievedAt { get; set; }
}

class WeatherForecastDbContext : DbContext
{
    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherForecast> Forecasts => Set<WeatherForecast>();
    public DbSet<CurrencyRate> Rates => Set<CurrencyRate>();
}

class WeatherService
{
    private readonly HttpClient _http;

    public WeatherService(HttpClient http)
    {
        _http = http;
    }

    public async Task<double?> GetKyivTemperatureAsync()
    {
        try
        {
            var url = "https://api.open-meteo.com/v1/forecast?latitude=50.45&longitude=30.52&current_weather=true";
            var resp = await _http.GetFromJsonAsync<OpenMeteoResponse>(url);
            return resp?.current_weather?.temperature;
        }
        catch
        {
            return null;
        }
    }

    private record OpenMeteoResponse(CurrentWeather current_weather)
    {
        public CurrentWeather current_weather { get; init; } = current_weather;
    }

    private record CurrentWeather(double temperature);
}

class CurrencyService
{
    private readonly HttpClient _http;

    public CurrencyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<Dictionary<string, double>> GetRatesAsync()
    {
        try
        {
            var url = "https://api.exchangerate.host/latest?base=USD&symbols=EUR,UAH";
            var resp = await _http.GetFromJsonAsync<ExchangeResponse>(url);
            return resp?.rates ?? new Dictionary<string, double>();
        }
        catch
        {
            return new Dictionary<string, double>();
        }
    }

    private record ExchangeResponse(Dictionary<string, double> rates)
    {
        public Dictionary<string, double> rates { get; init; } = rates;
    }
}
