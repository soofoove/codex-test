using System.ComponentModel.DataAnnotations;

namespace CodexTest.Entities;

public class WeatherForecastEntity
{
    [Key]
    public int Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}
