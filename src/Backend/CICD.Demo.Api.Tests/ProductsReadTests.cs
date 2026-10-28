using System.Net;
using System.Net.Http.Json;
using CICD.Demo.Api.Contracts;

namespace CICD.Demo.Api.Tests;

public sealed class ProductsReadTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ProductsReadTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetProducts_ReturnsSeededProducts()
    {
        var result = await GetPage("/api/products");

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(8, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetProducts_PaginatesResults()
    {
        var result = await GetPage("/api/products?page=2&pageSize=3");

        Assert.Equal(8, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Page);
        Assert.Equal([4, 5, 6], result.Items.Select(p => p.Id));
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=1000")]
    [InlineData("minPrice=-1")]
    [InlineData("minPrice=100&maxPrice=10")]
    public async Task GetProducts_WithInvalidQuery_ReturnsBadRequest(string query)
    {
        var response = await _client.GetAsync($"/api/products?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetProducts_SearchIsCaseInsensitive()
    {
        var result = await GetPage("/api/products?search=WIRELESS");

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, p => Assert.Contains("Wireless", p.Name));
    }

    [Fact]
    public async Task GetProducts_FiltersByCategoryAndStock()
    {
        var result = await GetPage("/api/products?category=electronics&inStock=true");

        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, p =>
        {
            Assert.Equal("Electronics", p.Category);
            Assert.True(p.Stock > 0);
        });
    }

    [Fact]
    public async Task GetProducts_SortsByPriceDescending()
    {
        var result = await GetPage("/api/products?sortBy=price&sortOrder=desc");

        var prices = result.Items.Select(p => p.Price).ToList();
        Assert.Equal(prices.OrderByDescending(p => p), prices);
    }

    [Fact]
    public async Task GetProductById_WhenProductExists_ReturnsProduct()
    {
        var response = await _client.GetAsync("/api/products/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

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
    public async Task GetCategories_ReturnsDistinctSortedCategories()
    {
        var categories = await _client.GetFromJsonAsync<List<string>>("/api/products/categories");

        Assert.Equal(["Accessories", "Electronics", "Home"], categories);
    }

    [Fact]
    public async Task GetStats_ReturnsCatalogueSummary()
    {
        var stats = await _client.GetFromJsonAsync<ProductStatsResponse>("/api/products/stats");

        Assert.NotNull(stats);
        Assert.Equal(8, stats.TotalProducts);
        Assert.Equal(6, stats.InStock);
        Assert.Equal(2, stats.OutOfStock);
        Assert.Equal(3, stats.Categories);
        Assert.True(stats.InventoryValue > 0);
    }

    private async Task<PagedResponse<ProductResponse>> GetPage(string url)
    {
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();

        Assert.NotNull(result);
        return result;
    }
}
