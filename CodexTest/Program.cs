using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/weather", async (string location, IConfiguration config, HttpClient http) =>
    {
        var key = config["OpenWeather:ApiKey"];
        if (string.IsNullOrWhiteSpace(key))
        {
            return Results.Problem("OpenWeather ApiKey is missing");
        }

        var url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(location)}&appid={key}&units=metric";

        try
        {
            var data = await http.GetFromJsonAsync<OpenWeatherResponse>(url);
            if (data is null)
            {
                return Results.NotFound();
            }
            return Results.Ok(new WeatherResult(data.Name, data.Main.Temp, data.Weather.FirstOrDefault()?.Description ?? string.Empty));
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    })
    .WithName("GetWeather");

app.MapGet("/currency", async (string? baseCurrency, HttpClient http) =>
    {
        baseCurrency ??= "USD";
        var url = $"https://api.exchangerate.host/latest?base={Uri.EscapeDataString(baseCurrency)}&symbols=USD,EUR,UAH";
        try
        {
            var data = await http.GetFromJsonAsync<ExchangeRateResponse>(url);
            return data is null ? Results.NotFound() : Results.Ok(data.Rates);
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    })
    .WithName("GetCurrency");

app.Run();

record OpenWeatherResponse(OpenWeatherMain Main, OpenWeatherWeather[] Weather, string Name);
record OpenWeatherMain(double Temp);
record OpenWeatherWeather(string Description);

record WeatherResult(string City, double TemperatureC, string Description);

record ExchangeRateResponse(Dictionary<string, decimal> Rates);
