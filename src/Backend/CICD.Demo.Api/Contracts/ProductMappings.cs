using CICD.Demo.Api.Models;

namespace CICD.Demo.Api.Contracts;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Category,
        product.Stock,
        product.IsActive,
        product.ImageUrl);
}
