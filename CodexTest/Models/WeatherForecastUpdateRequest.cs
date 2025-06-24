namespace CodexTest.Models;

public record WeatherForecastUpdateRequest(DateOnly Date, int TemperatureC, string? Summary);
