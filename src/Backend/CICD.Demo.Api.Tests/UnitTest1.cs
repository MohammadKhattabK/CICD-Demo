using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CICD.Demo.Api.Tests;


public class WeatherForecastApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveForecasts()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert: HTTP status
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Read JSON response
        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);
        var forecasts = document.RootElement;

        // Assert: response structure
        Assert.Equal(JsonValueKind.Array, forecasts.ValueKind);
        Assert.Equal(5, forecasts.GetArrayLength());

        foreach (var forecast in forecasts.EnumerateArray())
        {
            Assert.True(forecast.TryGetProperty("date", out _));
            Assert.True(forecast.TryGetProperty("temperatureC", out _));
            Assert.True(forecast.TryGetProperty("summary", out _));
            Assert.True(forecast.TryGetProperty("temperatureF", out _));
        }
    }
}
