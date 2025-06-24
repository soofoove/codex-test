using CodexTest.Domain;
using CodexTest.Mappings;
using CodexTest.Repositories;

namespace CodexTest.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherRepository _repository;

    public WeatherService(IWeatherRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateAsync(WeatherForecast forecast)
    {
        var id = await _repository.CreateAsync(forecast.ToEntity());
        forecast.Id = id;
        return id;
    }

    public async Task DeleteAsync(int id) => await _repository.DeleteAsync(id);

    public async Task<List<WeatherForecast>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(e => e.ToDomain()).ToList();

    public async Task<WeatherForecast?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity?.ToDomain();
    }

    public async Task UpdateAsync(WeatherForecast forecast) =>
        await _repository.UpdateAsync(forecast.ToEntity());
}
