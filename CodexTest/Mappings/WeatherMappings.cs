using CodexTest.Domain;
using CodexTest.Entities;
using CodexTest.Models;

namespace CodexTest.Mappings;

public static class WeatherMappings
{
    public static WeatherForecastEntity ToEntity(this WeatherForecast model) => new()
    {
        Id = model.Id,
        Date = model.Date,
        TemperatureC = model.TemperatureC,
        Summary = model.Summary
    };

    public static WeatherForecast ToDomain(this WeatherForecastEntity entity) => new()
    {
        Id = entity.Id,
        Date = entity.Date,
        TemperatureC = entity.TemperatureC,
        Summary = entity.Summary
    };

    public static WeatherForecast ToDomain(this WeatherForecastRequest request) => new()
    {
        Date = request.Date,
        TemperatureC = request.TemperatureC,
        Summary = request.Summary
    };

    public static WeatherForecast ToDomain(this WeatherForecastUpdateRequest request, int id) => new()
    {
        Id = id,
        Date = request.Date,
        TemperatureC = request.TemperatureC,
        Summary = request.Summary
    };

    public static WeatherForecastResponse ToResponse(this WeatherForecast model) => new()
    {
        Id = model.Id,
        Date = model.Date,
        TemperatureC = model.TemperatureC,
        Summary = model.Summary
    };
}
