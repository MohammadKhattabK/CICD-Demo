using System.Net;
using System.Net.Http.Json;
using CICD.Demo.Api.Contracts;

namespace CICD.Demo.Api.Tests;

public sealed class ProductsWriteTests : IClassFixture<ApiFactory>
{
    private static readonly ProductRequest ValidRequest = new(
        "Standing Desk", "Height adjustable desk", 399.90m, "Home", 5, "/images/desk.jpg");

    private readonly ApiFactory _factory;

    public ProductsWriteTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AdminCheck_ReflectsApiKeyValidity()
    {
        var withoutKey = await _factory.CreateClient().GetAsync("/api/admin/check");
        var withKey = await _factory.CreateAdminClient().GetAsync("/api/admin/check");

        Assert.Equal(HttpStatusCode.Unauthorized, withoutKey.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, withKey.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithoutApiKey_ReturnsUnauthorized()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/products", ValidRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithWrongApiKey_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", "wrong-key");

        var response = await client.PostAsJsonAsync("/api/products", ValidRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_WithValidRequest_PersistsProduct()
    {
        var admin = _factory.CreateAdminClient();

        var response = await admin.PostAsJsonAsync("/api/products", ValidRequest);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal("Standing Desk", created.Name);
        Assert.EndsWith($"/api/products/{created.Id}", response.Headers.Location?.ToString());

        var fetched = await _factory.CreateClient()
            .GetFromJsonAsync<ProductResponse>($"/api/products/{created.Id}");
        Assert.Equal(created, fetched);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidBody_ReturnsValidationProblem()
    {
        var invalid = new ProductRequest("", null, -5m, " ", -1, null);

        var response = await _factory.CreateAdminClient().PostAsJsonAsync("/api/products", invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>();
        Assert.NotNull(problem);
        Assert.Contains("name", problem.Errors.Keys);
        Assert.Contains("category", problem.Errors.Keys);
        Assert.Contains("price", problem.Errors.Keys);
        Assert.Contains("stock", problem.Errors.Keys);
    }

    [Fact]
    public async Task UpdateProduct_ReplacesFields()
    {
        var admin = _factory.CreateAdminClient();
        var created = await Create(admin);

        var update = ValidRequest with { Name = "Updated Desk", Price = 10m, Stock = 0 };
        var response = await admin.PutAsJsonAsync($"/api/products/{created.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Desk", updated.Name);
        Assert.Equal(10m, updated.Price);
        Assert.Equal(0, updated.Stock);
    }

    [Fact]
    public async Task UpdateProduct_WhenMissing_ReturnsNotFound()
    {
        var response = await _factory.CreateAdminClient()
            .PutAsJsonAsync("/api/products/99999", ValidRequest);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_DeactivatesIt()
    {
        var admin = _factory.CreateAdminClient();
        var created = await Create(admin);

        var delete = await admin.DeleteAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var fetch = await _factory.CreateClient().GetAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, fetch.StatusCode);

        var deleteAgain = await admin.DeleteAsync($"/api/products/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteAgain.StatusCode);
    }

    private static async Task<ProductResponse> Create(HttpClient admin)
    {
        var response = await admin.PostAsJsonAsync("/api/products", ValidRequest);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.NotNull(created);
        return created;
    }

    private sealed record ValidationProblem(Dictionary<string, string[]> Errors);
}
