using System.Net;
using System.Net.Http.Json;
using CodexTest.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CodexTest;

namespace CodexTest.Tests;

public class SqlServerFixture : IAsyncLifetime
{
    public MsSqlTestcontainer Container { get; }

    public SqlServerFixture()
    {
        Container = new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration { Password = "yourStrong(!)Password" })
            .Build();
    }

    public async Task InitializeAsync() => await Container.StartAsync();

    public async Task DisposeAsync() => await Container.DisposeAsync();
}

public class WeatherApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public WeatherApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<WeatherDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);
            services.AddDbContext<WeatherDbContext>(options => options.UseSqlServer(_connectionString));
        });
        return base.CreateHost(builder);
    }
}

public class WeatherForecastEndpointTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public WeatherForecastEndpointTests(SqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    private HttpClient CreateClient()
    {
        var factory = new WeatherApiFactory(_fixture.Container.ConnectionString);
        using var scope = factory.Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        ctx.Database.EnsureCreated();
        return factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesForecast()
    {
        using var client = CreateClient();
        var request = new WeatherForecastRequest(DateOnly.FromDateTime(DateTime.Today), 10, "Test");
        var response = await client.PostAsJsonAsync("/weatherforecast", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsForecasts()
    {
        using var client = CreateClient();
        var request = new WeatherForecastRequest(DateOnly.FromDateTime(DateTime.Today), 10, "Test");
        var post = await client.PostAsJsonAsync("/weatherforecast", request);
        var get = await client.GetAsync("/weatherforecast");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var forecasts = await get.Content.ReadFromJsonAsync<WeatherForecastResponse[]>();
        Assert.NotNull(forecasts);
        Assert.NotEmpty(forecasts!);
    }

    [Fact]
    public async Task Get_ById_ReturnsItem()
    {
        using var client = CreateClient();
        var request = new WeatherForecastRequest(DateOnly.FromDateTime(DateTime.Today), 10, "Test");
        var post = await client.PostAsJsonAsync("/weatherforecast", request);
        var location = post.Headers.Location!.ToString();
        var response = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Put_UpdatesItem()
    {
        using var client = CreateClient();
        var create = new WeatherForecastRequest(DateOnly.FromDateTime(DateTime.Today), 10, "Test");
        var post = await client.PostAsJsonAsync("/weatherforecast", create);
        var location = post.Headers.Location!.ToString();
        var id = int.Parse(location.Split('/').Last());
        var update = new WeatherForecastUpdateRequest(DateOnly.FromDateTime(DateTime.Today.AddDays(1)), 20, "Updated");
        var put = await client.PutAsJsonAsync($"/weatherforecast/{id}", update);
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        var get = await client.GetAsync($"/weatherforecast/{id}");
        var item = await get.Content.ReadFromJsonAsync<WeatherForecastResponse>();
        Assert.Equal(20, item!.TemperatureC);
    }

    [Fact]
    public async Task Delete_RemovesItem()
    {
        using var client = CreateClient();
        var create = new WeatherForecastRequest(DateOnly.FromDateTime(DateTime.Today), 10, "Test");
        var post = await client.PostAsJsonAsync("/weatherforecast", create);
        var id = int.Parse(post.Headers.Location!.Segments.Last());
        var delete = await client.DeleteAsync($"/weatherforecast/{id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        var get = await client.GetAsync($"/weatherforecast/{id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
