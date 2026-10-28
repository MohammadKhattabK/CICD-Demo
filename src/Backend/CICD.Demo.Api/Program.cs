using CICD.Demo.Api.Configuration;
using CICD.Demo.Api.Data;
using CICD.Demo.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<AdminApiKeyFilter>();

builder.Services.AddCors(options =>
{
    var origins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddDatabase();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    name = "CICD Demo Products API",
    version = "1.0.0",
    // Set by Render at runtime; lets smoke tests verify which commit is live.
    commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "unknown",
    status = "Running"
}));

app.MapProductEndpoints();
app.MapAdminEndpoints();

// Liveness has no dependencies; readiness also checks the database.
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

await app.InitializeDatabaseAsync();

app.Run();

public partial class Program { }
