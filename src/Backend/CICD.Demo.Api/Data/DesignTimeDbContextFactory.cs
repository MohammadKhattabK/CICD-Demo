using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CICD.Demo.Api.Data;

/// <summary>
/// Lets `dotnet ef` create migrations without a running database or app host.
/// Only the provider matters at design time; the connection is never opened.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=design_time")
            .Options;

        return new AppDbContext(options);
    }
}
