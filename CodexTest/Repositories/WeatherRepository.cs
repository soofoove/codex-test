using CodexTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodexTest.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly WeatherDbContext _context;

    public WeatherRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task<int> CreateAsync(WeatherForecastEntity forecast)
    {
        _context.Forecasts.Add(forecast);
        await _context.SaveChangesAsync();
        return forecast.Id;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Forecasts.FindAsync(id);
        if (entity is not null)
        {
            _context.Forecasts.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<WeatherForecastEntity>> GetAllAsync() => await _context.Forecasts.ToListAsync();

    public async Task<WeatherForecastEntity?> GetByIdAsync(int id) => await _context.Forecasts.FindAsync(id);

    public async Task UpdateAsync(WeatherForecastEntity forecast)
    {
        _context.Forecasts.Update(forecast);
        await _context.SaveChangesAsync();
    }
}
