using CodexTest.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodexTest;

public class WeatherDbContext : DbContext
{
    public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
    {
    }

    public DbSet<WeatherForecastEntity> Forecasts => Set<WeatherForecastEntity>();
}
