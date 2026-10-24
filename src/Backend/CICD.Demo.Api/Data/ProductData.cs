
using CICD.Demo.Api.Models;

namespace CICD.Demo.Api.Data;

public static class ProductData
{
    public static IReadOnlyList<Product> Products { get; } =
    [
       new(1, "Wireless Headphones", "سماعات لاسلكية بجودة صوت عالية",
    149.99m, "Electronics", 25, true, "/images/headphones.jpg"),

new(2, "Smart Watch", "ساعة ذكية لمتابعة النشاط اليومي",
    229.00m, "Electronics", 12, true, "/images/smartwatch.jpg"),

new(3, "Wireless Mouse", "فأرة لاسلكية مريحة للعمل",
    59.50m, "Electronics", 0, true, "/images/mouse.jpg"),

new(4, "Coffee Mug", "كوب قهوة بتصميم بسيط",
    25.50m, "Home", 40, true, "/images/coffee-mug.jpg"),

new(5, "Desk Lamp", "مصباح مكتب بإضاءة قابلة للتعديل",
    89.00m, "Home", 8, true, "/images/desk-lamp.jpg"),

new(6, "Travel Backpack", "حقيبة ظهر مناسبة للسفر والعمل",
    179.99m, "Accessories", 15, true, "/images/backpack.jpg"),

new(7, "Water Bottle", "قارورة مياه قابلة لإعادة الاستخدام",
    35.00m, "Accessories", 0, true, "/images/water-bottle.jpg"),

new(8, "Mechanical Keyboard", "لوحة مفاتيح ميكانيكية للكتابة والألعاب",
    299.00m, "Electronics", 7, true, "/images/keyboard.jpg")
    ];
}
