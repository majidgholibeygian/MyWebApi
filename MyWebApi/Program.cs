using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Minio;
using mywebapi.Models;
using mywebapi.Core.Abstractions;
using mywebapi.Infrastructure.Minio;
using mywebapi.Application.Storage;
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

// Register IMinioClient built from options
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

    var client = new MinioClient()
        .WithEndpoint(options.Endpoint)
        .WithCredentials(options.AccessKey, options.SecretKey)
        .WithSSL(options.Secure)
        .Build();

    return client;
});

// Infrastructure implements core abstraction
builder.Services.AddSingleton<IStorageService, MinioStorageService>();

// Application use-cases
builder.Services.AddScoped<IUploadFileUseCase, UploadFileUseCase>();

var app = builder.Build();

app.UseSwagger();
app.UseStaticFiles();

app.MapScalarApiReference(options =>
{
    options.OpenApiRoutePattern = "/swagger/v1/swagger.json";
    options.Title = "My API";
    options.Theme = ScalarTheme.Purple;
    options.EndpointPathPrefix = "docs";
});

app.UseAuthorization();
app.MapControllers();

app.Run();
