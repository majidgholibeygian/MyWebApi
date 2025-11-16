using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace mywebapi.Core.Abstractions
{
    // Storage abstraction for the application core (depend on abstractions, not implementations)
    public interface IStorageService
    {
        Task UploadAsync(Stream content, long size, string contentType, string bucketName, string objectName, CancellationToken cancellationToken = default);
        Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    }
}