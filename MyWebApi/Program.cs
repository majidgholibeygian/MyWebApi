using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Minio;
using mywebapi.Models;
using mywebapi.Services;
using mywebapi.Swagger;
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

    c.OperationFilter<FileUploadOperationFilter>();
});

// Bind MinIO options (can be overridden via appsettings.json or env vars)
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection("Minio"));

// Register an IMinioClient built from options
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

    // Build the Minio client using the fluent builder. Build() returns IMinioClient.
    var client = new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .WithSSL(options.Secure)
        .Build();

    return client;
});

// Register storage abstraction and implementation (dependency inversion)
builder.Services.AddSingleton<IStorageService, MinioStorageService>();

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

app.UseAuthorization();
app.MapControllers();

app.Run();
