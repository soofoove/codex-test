using CodexTest;
using CodexTest.Mappings;
using CodexTest.Models;
using CodexTest.Repositories;
using CodexTest.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<WeatherDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IWeatherRepository, WeatherRepository>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/weatherforecast", async (IWeatherService service) =>
{
    var forecasts = await service.GetAllAsync();
    return forecasts.Select(f => f.ToResponse());
}).WithName("GetWeatherForecast");

app.MapGet("/weatherforecast/{id}", async (int id, IWeatherService service) =>
{
    var forecast = await service.GetByIdAsync(id);
    return forecast is null ? Results.NotFound() : Results.Ok(forecast.ToResponse());
}).WithName("GetWeatherForecastById");

app.MapPost("/weatherforecast", async (WeatherForecastRequest request, IWeatherService service) =>
{
    var id = await service.CreateAsync(request.ToDomain());
    return Results.Created($"/weatherforecast/{id}", null);
}).WithName("CreateWeatherForecast");

app.MapPut("/weatherforecast/{id}", async (int id, WeatherForecastUpdateRequest request, IWeatherService service) =>
{
    var existing = await service.GetByIdAsync(id);
    if (existing is null)
        return Results.NotFound();

    var model = request.ToDomain(id);
    await service.UpdateAsync(model);
    return Results.NoContent();
}).WithName("UpdateWeatherForecast");

app.MapDelete("/weatherforecast/{id}", async (int id, IWeatherService service) =>
{
    var existing = await service.GetByIdAsync(id);
    if (existing is null)
        return Results.NotFound();

    await service.DeleteAsync(id);
    return Results.NoContent();
}).WithName("DeleteWeatherForecast");

app.Run();

public partial class Program { }
