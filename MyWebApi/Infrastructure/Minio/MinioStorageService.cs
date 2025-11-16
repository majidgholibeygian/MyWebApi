using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Options;
using mywebapi.Core.Abstractions;
using mywebapi.Models;

namespace mywebapi.Infrastructure.Minio
{
    // Infrastructure implementation depends on Minio SDK and implements the core abstraction.
    public class MinioStorageService : IStorageService
    {
        private readonly IMinioClient _client;
        private readonly MinioOptions _options;

        public MinioStorageService(IMinioClient client, IOptions<MinioOptions> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task UploadAsync(Stream content, long size, string contentType, string bucketName, string objectName, CancellationToken cancellationToken = default)
        {
            if (content is null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(bucketName)) throw new ArgumentException("bucketName is required", nameof(bucketName));
            if (string.IsNullOrWhiteSpace(objectName)) throw new ArgumentException("objectName is required", nameof(objectName));

            // Ensure bucket exists
            var bucketExists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName), cancellationToken);
            if (!bucketExists)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName), cancellationToken);
            }

            // If the incoming stream is at position 0 but length unknown, prefer passing size if available.
            var putArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(content)
                .WithObjectSize(size)
                .WithContentType(contentType);

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