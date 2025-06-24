using CodexTest.Domain;

namespace CodexTest.Services;

public interface IWeatherService
{
    Task<List<WeatherForecast>> GetAllAsync();
    Task<WeatherForecast?> GetByIdAsync(int id);
    Task<int> CreateAsync(WeatherForecast forecast);
    Task UpdateAsync(WeatherForecast forecast);
    Task DeleteAsync(int id);
}
