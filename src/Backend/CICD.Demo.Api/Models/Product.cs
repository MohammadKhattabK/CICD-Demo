
namespace CICD.Demo.Api.Models;

public sealed record Product(
    int Id,
    string Name,
    string Description,
    decimal Price,
    string Category,
    int Stock,
    bool IsActive,
    string ImageUrl);
