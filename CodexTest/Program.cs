using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.JsonPatch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddDbContext<WeatherForecastDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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
