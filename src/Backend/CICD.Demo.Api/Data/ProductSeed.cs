using CICD.Demo.Api.Models;

namespace CICD.Demo.Api.Data;

/// <summary>Initial catalogue, applied through the first migration.</summary>
public static class ProductSeed
{
    // Fixed so that the seed (and therefore the migration) is deterministic.
    private static readonly DateTimeOffset SeededAt =
        new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static IReadOnlyList<Product> Products { get; } =
    [
        Create(1, "Wireless Headphones", "سماعات لاسلكية بجودة صوت عالية",
            149.99m, "Electronics", 25, "/images/headphones.jpg"),
        Create(2, "Smart Watch", "ساعة ذكية لمتابعة النشاط اليومي",
            229.00m, "Electronics", 12, "/images/smartwatch.jpg"),
        Create(3, "Wireless Mouse", "فأرة لاسلكية مريحة للعمل",
            59.50m, "Electronics", 0, "/images/mouse.jpg"),
        Create(4, "Coffee Mug", "كوب قهوة بتصميم بسيط",
            25.50m, "Home", 40, "/images/coffee-mug.jpg"),
        Create(5, "Desk Lamp", "مصباح مكتب بإضاءة قابلة للتعديل",
            89.00m, "Home", 8, "/images/desk-lamp.jpg"),
        Create(6, "Travel Backpack", "حقيبة ظهر مناسبة للسفر والعمل",
            179.99m, "Accessories", 15, "/images/backpack.jpg"),
        Create(7, "Water Bottle", "قارورة مياه قابلة لإعادة الاستخدام",
            35.00m, "Accessories", 0, "/images/water-bottle.jpg"),
        Create(8, "Mechanical Keyboard", "لوحة مفاتيح ميكانيكية للكتابة والألعاب",
            299.00m, "Electronics", 7, "/images/keyboard.jpg")
    ];

    private static Product Create(
        int id, string name, string description, decimal price,
        string category, int stock, string imageUrl) => new()
    {
        Id = id,
        Name = name,
        Description = description,
        Price = price,
        Category = category,
        Stock = stock,
        IsActive = true,
        ImageUrl = imageUrl,
        CreatedAt = SeededAt,
        UpdatedAt = SeededAt
    };
}
