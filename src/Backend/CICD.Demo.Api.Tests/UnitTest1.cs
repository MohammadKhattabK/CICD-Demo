
using System.Net;
using System.Net.Http.Json;
using CICD.Demo.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CICD.Demo.Api.Tests;

public sealed class ProductsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ProductsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<ProductsResponse>();

        Assert.NotNull(result);
        Assert.True(result.TotalCount > 0);
        // Assert.True(result.TotalCount < 0);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsProduct()
    {
        var response = await _client.GetAsync("/api/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<Product>();

        Assert.NotNull(product);
        Assert.Equal(1, product.Id);
    }

    [Fact]
    public async Task GetProductById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/products/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCategories_ReturnsCategories()
    {
        var response = await _client.GetAsync("/api/products/categories");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var categories = await response.Content
            .ReadFromJsonAsync<List<string>>();

        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
    }

    
    [Fact]
    public async Task GetHealthLive_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetHealthReady_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }


    private sealed record ProductsResponse(
        int TotalCount,
        IReadOnlyList<Product> Items);
}
