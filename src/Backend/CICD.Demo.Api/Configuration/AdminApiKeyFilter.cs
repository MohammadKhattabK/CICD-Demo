using System.Security.Cryptography;
using System.Text;

namespace CICD.Demo.Api.Configuration;

/// <summary>
/// Endpoint filter protecting write operations with the X-Api-Key header.
/// The expected key comes from configuration (Admin:ApiKey / Admin__ApiKey).
/// </summary>
public sealed class AdminApiKeyFilter(IConfiguration configuration) : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var expected = configuration["Admin:ApiKey"];

        if (string.IsNullOrWhiteSpace(expected))
        {
            return Results.Problem(
                title: "Write operations are disabled.",
                detail: "No admin API key is configured on the server.",
                statusCode: StatusCodes.Status403Forbidden);
        }

        var provided = context.HttpContext.Request.Headers[HeaderName].ToString();

        if (!FixedTimeEquals(provided, expected))
        {
            return Results.Problem(
                title: "Unauthorized.",
                detail: $"A valid {HeaderName} header is required.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        return await next(context);
    }

    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(left)),
            SHA256.HashData(Encoding.UTF8.GetBytes(right)));
}
