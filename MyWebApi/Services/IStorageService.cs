using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace mywebapi.Services
{
    // Single responsibility: contract for storage operations.
    public interface IStorageService
    {
        Task UploadAsync(IFormFile file, string bucketName, string objectName, CancellationToken cancellationToken = default);
        Task<Stream> DownloadAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    }
}