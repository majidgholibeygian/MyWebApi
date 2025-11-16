using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using mywebapi.Core.Abstractions;
using mywebapi.Models;

namespace mywebapi.Application.Storage
{
    // Use-case implementation: coordinates input validation and calls the storage abstraction.
    public class UploadFileUseCase : IUploadFileUseCase
    {
        private readonly IStorageService _storage;
        private readonly MinioOptions _options;

        public UploadFileUseCase(IStorageService storage, Microsoft.Extensions.Options.IOptions<MinioOptions> options)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<(string Bucket, string ObjectName)> ExecuteAsync(IFormFile file, string? bucket = null, CancellationToken cancellationToken = default)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            var bucketName = string.IsNullOrWhiteSpace(bucket) ? _options.BucketName : bucket;
            var objectName = file.FileName ?? Guid.NewGuid().ToString();

            using var stream = file.OpenReadStream();
            await _storage.UploadAsync(stream, file.Length, file.ContentType ?? "application/octet-stream", bucketName, objectName, cancellationToken);

            return (bucketName, objectName);
        }
    }
}