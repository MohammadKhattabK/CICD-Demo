using CICD.Demo.Api.Models;
using CICD.Demo.Api.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CICD.Demo.Api.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(ProductRequestValidator.MaxNameLength)
            .IsRequired();

        builder.Property(product => product.Description)
            .HasMaxLength(ProductRequestValidator.MaxDescriptionLength)
            .IsRequired();

        builder.Property(product => product.Category)
            .HasMaxLength(ProductRequestValidator.MaxCategoryLength)
            .IsRequired();

        builder.Property(product => product.ImageUrl)
            .HasMaxLength(ProductRequestValidator.MaxImageUrlLength)
            .IsRequired();

        builder.Property(product => product.Price)
            .HasPrecision(18, 2);

        builder.HasIndex(product => product.Category);
        builder.HasIndex(product => product.IsActive);

        builder.HasData(ProductSeed.Products);
    }
}
