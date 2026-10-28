using CICD.Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CICD.Demo.Api.Configuration;

public static class DatabaseExtensions
{
    public const string MissingConnectionMessage =
        "No database connection configured. Set ConnectionStrings__Default " +
        "or DATABASE_URL (postgres://user:password@host:port/database).";

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        // The connection string is resolved lazily so that configuration
        // supplied late (e.g. by integration tests) is honoured.
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var connectionString = ResolveConnectionString(configuration)
                ?? throw new InvalidOperationException(MissingConnectionMessage);

            options.UseNpgsql(connectionString);
        });

        return services;
    }

    /// <summary>
    /// Reads ConnectionStrings:Default, or falls back to the DATABASE_URL
    /// variable that hosts such as Render provide.
    /// </summary>
    public static string? ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString;

        var databaseUrl = configuration["DATABASE_URL"];
        return string.IsNullOrWhiteSpace(databaseUrl)
            ? null
            : FromDatabaseUrl(databaseUrl);
    }

    public static string FromDatabaseUrl(string databaseUrl)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("postgres" or "postgresql"))
        {
            // Already a key=value connection string.
            return databaseUrl;
        }

        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort || uri.Port <= 0 ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            // Render's internal network does not use TLS, its external endpoint does.
            SslMode = SslMode.Prefer
        };

        return builder.ConnectionString;
    }

    /// <summary>Applies pending migrations (relational) or creates the seeded in-memory store.</summary>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (!db.Database.IsRelational())
        {
            await db.Database.EnsureCreatedAsync();
            return;
        }

        if (app.Configuration.GetValue("Database:MigrateOnStartup", true))
        {
            app.Logger.LogInformation("Applying database migrations...");
            await db.Database.MigrateAsync();
        }
    }
}
