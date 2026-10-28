using CICD.Demo.Api.Configuration;
using CICD.Demo.Api.Contracts;
using CICD.Demo.Api.Data;
using CICD.Demo.Api.Models;
using CICD.Demo.Api.Validation;
using Microsoft.EntityFrameworkCore;

namespace CICD.Demo.Api.Endpoints;

public static class ProductEndpoints
{
    private const int DefaultPageSize = 12;
    private const int MaxPageSize = 50;

    public static IEndpointRouteBuilder MapProductEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products");

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get products with search, filtering, sorting and pagination.");

        group.MapGet("/categories", GetCategories)
            .WithName("GetProductCategories")
            .WithSummary("Get available product categories.");

        group.MapGet("/stats", GetStats)
            .WithName("GetProductStats")
            .WithSummary("Get catalogue statistics.");

        group.MapGet("/{id:int}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by its ID.");

        group.MapPost("/", CreateProduct)
            .AddEndpointFilter<AdminApiKeyFilter>()
            .WithName("CreateProduct")
            .WithSummary("Create a product (requires X-Api-Key).");

        group.MapPut("/{id:int}", UpdateProduct)
            .AddEndpointFilter<AdminApiKeyFilter>()
            .WithName("UpdateProduct")
            .WithSummary("Replace a product (requires X-Api-Key).");

        group.MapDelete("/{id:int}", DeleteProduct)
            .AddEndpointFilter<AdminApiKeyFilter>()
            .WithName("DeleteProduct")
            .WithSummary("Deactivate a product (requires X-Api-Key).");

        return app;
    }

    private static async Task<IResult> GetProducts(
        AppDbContext db,
        string? search,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        bool? inStock,
        string? sortBy,
        string? sortOrder,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        if (minPrice < 0 || maxPrice < 0)
        {
            return Results.BadRequest(new { message = "Prices cannot be negative." });
        }

        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
        {
            return Results.BadRequest(new { message = "minPrice cannot exceed maxPrice." });
        }

        if (page < 1)
        {
            return Results.BadRequest(new { message = "page must be 1 or greater." });
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            return Results.BadRequest(new
            {
                message = $"pageSize must be between 1 and {MaxPageSize}."
            });
        }

        var currentPage = page ?? 1;
        var currentPageSize = pageSize ?? DefaultPageSize;

        var query = db.Products.AsNoTracking().Where(product => product.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(product =>
                product.Name.ToLower().Contains(term) ||
                product.Description.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var categoryName = category.Trim().ToLower();
            query = query.Where(product => product.Category.ToLower() == categoryName);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(product => product.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(product => product.Price <= maxPrice.Value);
        }

        if (inStock == true)
        {
            query = query.Where(product => product.Stock > 0);
        }
        else if (inStock == false)
        {
            query = query.Where(product => product.Stock == 0);
        }

        var descending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        // Id is the tie-breaker so pages are stable.
        query = (sortBy ?? "id").ToLowerInvariant() switch
        {
            "price" => descending
                ? query.OrderByDescending(p => p.Price).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "name" => descending
                ? query.OrderByDescending(p => p.Name).ThenBy(p => p.Id)
                : query.OrderBy(p => p.Name).ThenBy(p => p.Id),
            _ => descending
                ? query.OrderByDescending(p => p.Id)
                : query.OrderBy(p => p.Id)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((currentPage - 1) * currentPageSize)
            .Take(currentPageSize)
            .Select(product => new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Category,
                product.Stock,
                product.IsActive,
                product.ImageUrl))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)currentPageSize);

        return Results.Ok(new PagedResponse<ProductResponse>(
            totalCount, currentPage, currentPageSize, totalPages, items));
    }

    private static async Task<IResult> GetCategories(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var categories = await db.Products
            .AsNoTracking()
            .Where(product => product.IsActive)
            .Select(product => product.Category)
            .Distinct()
            .OrderBy(category => category)
            .ToListAsync(cancellationToken);

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetStats(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var active = db.Products.AsNoTracking().Where(product => product.IsActive);

        var total = await active.CountAsync(cancellationToken);
        var inStock = await active.CountAsync(product => product.Stock > 0, cancellationToken);
        var categories = await active.Select(product => product.Category)
            .Distinct().CountAsync(cancellationToken);
        var inventoryValue = await active
            .SumAsync(product => product.Price * product.Stock, cancellationToken);

        return Results.Ok(new ProductStatsResponse(
            total, inStock, total - inStock, categories, inventoryValue));
    }

    private static async Task<IResult> GetProductById(
        int id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

        return product is null
            ? NotFound(id)
            : Results.Ok(product.ToResponse());
    }

    private static async Task<IResult> CreateProduct(
        ProductRequest request,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var errors = ProductRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var now = DateTimeOffset.UtcNow;
        var product = new Product { CreatedAt = now, UpdatedAt = now, IsActive = true };
        Apply(product, request);

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return Results.CreatedAtRoute(
            "GetProductById", new { id = product.Id }, product.ToResponse());
    }

    private static async Task<IResult> UpdateProduct(
        int id,
        ProductRequest request,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var errors = ProductRequestValidator.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

        if (product is null)
        {
            return NotFound(id);
        }

        Apply(product, request);
        product.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(product.ToResponse());
    }

    private static async Task<IResult> DeleteProduct(
        int id,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

        if (product is null)
        {
            return NotFound(id);
        }

        // Soft delete keeps history and keeps existing references valid.
        product.IsActive = false;
        product.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }

    private static void Apply(Product product, ProductRequest request)
    {
        product.Name = request.Name!.Trim();
        product.Description = request.Description?.Trim() ?? string.Empty;
        product.Price = request.Price;
        product.Category = request.Category!.Trim();
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl?.Trim() ?? string.Empty;
    }

    private static IResult NotFound(int id) =>
        Results.NotFound(new { message = $"Product with ID {id} was not found." });
}
