using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1",
        Description = "Sample API using Scalar UI"
    });
});

var app = builder.Build();

app.UseSwagger(); // serves /swagger/v1/swagger.json

app.UseStaticFiles(); // optional if you serve local assets like logos

// Use Scalar UI (requires the correct package + using directive)
app.MapScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "/swagger/v1/swagger.json";
    options.Title = "My API";
    options.Theme = ScalarTheme.Purple;
    options.EndpointPathPrefix = "docs"; // /docs
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
