
using CICD.Demo.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "https://cicd-demo-dzry.onrender.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

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
    status = "Running"
}));

app.MapProductEndpoints();


app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program { }
