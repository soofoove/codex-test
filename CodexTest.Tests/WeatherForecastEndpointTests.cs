using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using CodexTest;

namespace CodexTest.Tests;

public class WeatherForecastEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsSuccessAndFiveRecords()
    {
        var response = await _client.GetAsync("/weatherforecast");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var forecasts = await response.Content.ReadFromJsonAsync<TestForecast[]>();
        Assert.NotNull(forecasts);
        Assert.Equal(5, forecasts!.Length);
    }

    private record TestForecast(DateOnly Date, int TemperatureC, string? Summary);
}

