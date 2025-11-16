using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonSchemaType = Microsoft.OpenApi.JsonSchemaType;

namespace mywebapi.Swagger
{
    // Convert IFormFile / IFormFileCollection parameters into multipart/form-data requestBody
    public class FileUploadOperationFilter : IOperationFilter
    {
        public FileUploadOperationFilter()
        {
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation == null || context == null) return;

            var fileParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFileCollection))
                .ToList();

            if (!fileParams.Any()) return;

            // Ensure parameters list exists and remove file parameters from it
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IOpenApiParameter>();
            }

            var fileNames = new HashSet<string>(fileParams.Select(p => p.Name ?? string.Empty), StringComparer.OrdinalIgnoreCase);
            operation.Parameters = operation.Parameters
                .Where(p => !fileNames.Contains(p.Name ?? string.Empty))
                .ToList();

            // Build multipart/form-data schema (use concrete OpenApiSchema)
            var schema = new OpenApiSchema
            {
                Type = Microsoft.OpenApi.JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>()
            };

            foreach (var p in fileParams)
            {
                var name = p.Name ?? throw new InvalidOperationException("Parameter name is required for file parameters.");
                schema.Properties[name] = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Format = "binary"
                };
            }

            // Include other [FromForm] simple parameters as strings/ints/bools (best-effort)
            var nonFileFormParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.GetCustomAttribute<FromFormAttribute>() != null
                            && p.ParameterType != typeof(IFormFile)
                            && p.ParameterType != typeof(IFormFileCollection));

            foreach (var p in nonFileFormParams)
            {
                var name = p.Name;
                if (string.IsNullOrEmpty(name))
                    continue;
                if (!schema.Properties.ContainsKey(name))
                {
                    schema.Properties[name] = p.ParameterType == typeof(string)
                        ? new OpenApiSchema { Type = JsonSchemaType.String }
                        : p.ParameterType == typeof(int) || p.ParameterType == typeof(long)
                            ? new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" }
                            : p.ParameterType == typeof(bool)
                                ? new OpenApiSchema { Type = JsonSchemaType.Boolean }
                                : new OpenApiSchema { Type = JsonSchemaType.String };
                }
            }

            // Ensure RequestBody and its Content dictionary are initialized before adding entries
            if (operation.RequestBody == null)
            {
                operation.RequestBody = new OpenApiRequestBody();
            }

            // Fix: Do not assign to Content, instead add or update the entry directly
            var content = operation.RequestBody.Content;
            if (content == null)
            {
                // If Content is null, create a new dictionary and add the entry via reflection
                var contentProperty = operation.RequestBody.GetType().GetProperty("Content");
                if (contentProperty != null && contentProperty.CanWrite)
                {
                    var newContent = new Dictionary<string, OpenApiMediaType>();
                    newContent["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = schema
                    };
                    contentProperty.SetValue(operation.RequestBody, newContent);
                }
            }
            else
            {
                content["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = schema
                };
            }
        }
    }
}