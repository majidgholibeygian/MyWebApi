using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using mywebapi.Models;

namespace mywebapi.Services
{
    // Concrete implementation that talks to MinIO.
    // Follows dependency inversion: depends on abstractions (IOptions<MinioOptions>) and IMinioClient injected via DI.
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _client;
        private readonly MinioOptions _options;

        public MinioStorageService(IMinioClient client, IOptions<MinioOptions> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task UploadAsync(IFormFile file, string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentException("bucketName is required", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(objectName)) throw new ArgumentException("objectName is required", nameof(objectName));

            // Ensure bucket exists
            var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
            if (!bucketExists)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken);
            }

            using var stream = file.OpenReadStream();
            var putArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(file.ContentType ?? "application/octet-stream");

            await _client.PutObjectAsync(putArgs, cancellationToken);
        }

        public async Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentException("bucketName is required", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(objectName)) throw new ArgumentException("objectName is required", nameof(objectName));

            var ms = new MemoryStream();
            var getArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(stream => stream.CopyTo(ms));

            await _client.GetObjectAsync(getArgs, cancellationToken);
            ms.Position = 0;
            return ms;
        }
    }
}