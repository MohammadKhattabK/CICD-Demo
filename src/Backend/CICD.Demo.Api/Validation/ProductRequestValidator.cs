using CICD.Demo.Api.Contracts;

namespace CICD.Demo.Api.Validation;

public static class ProductRequestValidator
{
    public const int MaxNameLength = 120;
    public const int MaxDescriptionLength = 1000;
    public const int MaxCategoryLength = 60;
    public const int MaxImageUrlLength = 500;

    /// <summary>Returns validation errors keyed by field, or an empty dictionary if valid.</summary>
    public static Dictionary<string, string[]> Validate(ProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors["name"] = ["Name is required."];
        else if (request.Name.Trim().Length > MaxNameLength)
            errors["name"] = [$"Name cannot exceed {MaxNameLength} characters."];

        if (request.Description?.Length > MaxDescriptionLength)
            errors["description"] = [$"Description cannot exceed {MaxDescriptionLength} characters."];

        if (string.IsNullOrWhiteSpace(request.Category))
            errors["category"] = ["Category is required."];
        else if (request.Category.Trim().Length > MaxCategoryLength)
            errors["category"] = [$"Category cannot exceed {MaxCategoryLength} characters."];

        if (request.Price < 0 || request.Price > 1_000_000)
            errors["price"] = ["Price must be between 0 and 1,000,000."];

        if (request.Stock < 0)
            errors["stock"] = ["Stock cannot be negative."];

        if (request.ImageUrl?.Length > MaxImageUrlLength)
            errors["imageUrl"] = [$"ImageUrl cannot exceed {MaxImageUrlLength} characters."];

        return errors;
    }
}
