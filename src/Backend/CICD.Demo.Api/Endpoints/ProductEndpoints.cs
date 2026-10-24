
using CICD.Demo.Api.Data;
using CICD.Demo.Api.Models;

namespace CICD.Demo.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get products with search, filtering and sorting.");

        group.MapGet("/categories", GetCategories)
            .WithName("GetProductCategories")
            .WithSummary("Get available product categories.");

        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by its ID.");

        return app;
    }

    private static IResult GetProducts(
        string? search,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        string? sortOrder)
    {
        if (minPrice < 0 || maxPrice < 0)
        {
            return Results.BadRequest(new
            {
                message = "Prices cannot be negative."
            });
        }

        if (minPrice.HasValue &&
            maxPrice.HasValue &&
            minPrice.Value > maxPrice.Value)
        {
            return Results.BadRequest(new
            {
                message = "minPrice cannot exceed maxPrice."
            });
        }

        IEnumerable<Product> query = ProductData.Products
            .Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
                product.Name.Contains(
                    search, StringComparison.OrdinalIgnoreCase) ||
                product.Description.Contains(
                    search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(product =>
                product.Category.Equals(
                    category, StringComparison.OrdinalIgnoreCase));
        }

        if (minPrice.HasValue)
        {
            query = query.Where(product =>
                product.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(product =>
                product.Price <= maxPrice.Value);
        }

        if (inStock == true)
        {
            query = query.Where(product => product.Stock > 0);
        }
        else if (inStock == false)
        {
            query = query.Where(product => product.Stock == 0);
        }

        var descending = string.Equals(
            sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy ?? "id").ToLowerInvariant() switch
        {
            "price" => descending
                ? query.OrderByDescending(product => product.Price)
                : query.OrderBy(product => product.Price),

            "name" => descending
                ? query.OrderByDescending(product => product.Name)
                : query.OrderBy(product => product.Name),

            _ => descending
                ? query.OrderByDescending(product => product.Id)
                : query.OrderBy(product => product.Id)
        };

        var items = query.ToArray();

        return Results.Ok(new
        {
            totalCount = items.Length,
            items
        });
    }

    private static IResult GetCategories()
    {
        var categories = ProductData.Products
            .Where(product => product.IsActive)
            .Select(product => product.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(category => category)
            .ToArray();

        return Results.Ok(categories);
    }

    private static IResult GetProductById(int id)
    {
        var product = ProductData.Products.FirstOrDefault(
            product => product.Id == id && product.IsActive);

        return product is null
            ? Results.NotFound(new
            {
                message = $"Product with ID {id} was not found."
            })
            : Results.Ok(product);
    }
}
