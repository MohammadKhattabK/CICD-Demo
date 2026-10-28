namespace CICD.Demo.Api.Contracts;

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int Stock,
    bool IsActive,
    string ImageUrl);

/// <summary>Body for creating (POST) or fully replacing (PUT) a product.</summary>
public sealed record ProductRequest(
    string? Name,
    string? Description,
    decimal Price,
    string? Category,
    int Stock,
    string? ImageUrl);

public sealed record PagedResponse<T>(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyList<T> Items);

public sealed record ProductStatsResponse(
    int TotalProducts,
    int InStock,
    int OutOfStock,
    int Categories,
    decimal InventoryValue);
