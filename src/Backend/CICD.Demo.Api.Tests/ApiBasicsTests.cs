using System.Net;
using System.Net.Http.Json;

namespace CICD.Demo.Api.Tests;

public sealed class ApiBasicsTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ApiBasicsTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetRoot_ReturnsRunningStatusAndCommit()
    {
        var response = await _client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var info = await response.Content.ReadFromJsonAsync<RootResponse>();

        Assert.NotNull(info);
        Assert.Equal("Running", info.Status);
        Assert.False(string.IsNullOrWhiteSpace(info.Commit));
    }

    [Fact]
    public async Task GetHealthLive_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetHealthReady_ChecksDatabaseAndReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    private sealed record RootResponse(string Name, string Version, string Commit, string Status);
}
