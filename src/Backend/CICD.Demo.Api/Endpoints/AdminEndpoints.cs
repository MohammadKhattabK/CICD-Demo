using CICD.Demo.Api.Configuration;

namespace CICD.Demo.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(
        this IEndpointRouteBuilder app)
    {
        // Lets a client check an admin key before attempting any write.
        app.MapGet("/api/admin/check", () => Results.NoContent())
            .AddEndpointFilter<AdminApiKeyFilter>()
            .WithTags("Admin")
            .WithName("CheckAdminKey")
            .WithSummary("Returns 204 when the X-Api-Key header is valid.");

        return app;
    }
}
