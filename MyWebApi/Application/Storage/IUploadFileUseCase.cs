using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace mywebapi.Application.Storage
{
    // Application layer: small use-case interface used by controllers
    public interface IUploadFileUseCase
    {
        Task<(string Bucket, string ObjectName)> ExecuteAsync(IFormFile file, string? bucket = null, CancellationToken cancellationToken = default);
    }
}