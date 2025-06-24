using CodexTest.Entities;

namespace CodexTest.Repositories;

public interface IWeatherRepository
{
    Task<List<WeatherForecastEntity>> GetAllAsync();
    Task<WeatherForecastEntity?> GetByIdAsync(int id);
    Task<int> CreateAsync(WeatherForecastEntity forecast);
    Task UpdateAsync(WeatherForecastEntity forecast);
    Task DeleteAsync(int id);
}
