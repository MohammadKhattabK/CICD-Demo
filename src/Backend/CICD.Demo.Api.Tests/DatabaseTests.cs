using CICD.Demo.Api.Configuration;
using CICD.Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CICD.Demo.Api.Tests;

public sealed class DatabaseTests
{
    [Fact]
    public void FromDatabaseUrl_ConvertsRenderStyleUrl()
    {
        var connectionString = DatabaseExtensions.FromDatabaseUrl(
            "postgres://app_user:p%40ss:word@dpg-abc.example.com:5433/appdb");

        Assert.Contains("Host=dpg-abc.example.com", connectionString);
        Assert.Contains("Port=5433", connectionString);
        Assert.Contains("Database=appdb", connectionString);
        Assert.Contains("Username=app_user", connectionString);
        Assert.Contains("Password=p@ss:word", connectionString);
    }

    [Fact]
    public void FromDatabaseUrl_UsesDefaultPortWhenOmitted()
    {
        var connectionString = DatabaseExtensions.FromDatabaseUrl("postgresql://u:p@host/db");

        Assert.Contains("Port=5432", connectionString);
    }

    [Fact]
    public void FromDatabaseUrl_LeavesKeyValueConnectionStringsUntouched()
    {
        const string keyValue = "Host=localhost;Database=x;Username=u;Password=p";

        Assert.Equal(keyValue, DatabaseExtensions.FromDatabaseUrl(keyValue));
    }

    [Fact]
    public void ResolveConnectionString_PrefersExplicitConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=explicit",
                ["DATABASE_URL"] = "postgres://u:p@fallback/db"
            })
            .Build();

        Assert.Equal("Host=explicit", DatabaseExtensions.ResolveConnectionString(configuration));
    }

    [Fact]
    public void ResolveConnectionString_ReturnsNullWhenNothingConfigured()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Null(DatabaseExtensions.ResolveConnectionString(configuration));
    }

    [Fact]
    public void Model_HasNoPendingChangesWithoutAMigration()
    {
        using var db = new DesignTimeDbContextFactory().CreateDbContext([]);

        Assert.False(
            db.Database.HasPendingModelChanges(),
            "The model changed but no migration was added. Run `dotnet ef migrations add <Name>`.");
    }

    [PostgresFact]
    public async Task Migrations_ApplyToRealPostgres_AndSeedCatalogue()
    {
        var connectionString = Environment.GetEnvironmentVariable(PostgresFactAttribute.VariableName)!;
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        await using var db = new AppDbContext(options);

        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Equal(8, await db.Products.CountAsync());

        await db.Database.EnsureDeletedAsync();
    }
}

/// <summary>Runs only when TEST_POSTGRES_CONNECTION points at a throwaway PostgreSQL database.</summary>
public sealed class PostgresFactAttribute : FactAttribute
{
    public const string VariableName = "TEST_POSTGRES_CONNECTION";

    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(VariableName)))
        {
            Skip = $"Set {VariableName} to run the PostgreSQL migration test.";
        }
    }
}
