using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using mywebapi.Application.Storage;

namespace mywebapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IUploadFileUseCase _uploadUseCase;

        public StorageController(IUploadFileUseCase uploadUseCase)
        {
            _uploadUseCase = uploadUseCase;
        }

        /// <summary>
        /// Upload a file to MinIO. Form-data: file (IFormFile). Optional query: bucket.
        /// </summary>
        [HttpPost("upload")]        
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? bucket)
        {
            if (file == null) return BadRequest("file is required");

            var result = await _uploadUseCase.ExecuteAsync(file, bucket);
            return Ok(new { bucket = result.Bucket, objectName = result.ObjectName });
        }

        /// <summary>
        /// Download a file from MinIO. Query: objectName, optional bucket.
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string objectName, [FromQuery] string? bucket)
        {
            if (string.IsNullOrWhiteSpace(objectName)) return BadRequest("objectName is required");

            // For download we use the infrastructure abstraction directly (or create a Download use-case similarly)
            // Keep controller thin: if you need auth/validation, put it in use-case.
            var bucketName = string.IsNullOrWhiteSpace(bucket) ? "uploads" : bucket;
            var stream = await HttpContext.RequestServices.GetRequiredService<mywebapi.Core.Abstractions.IStorageService>()
                                  .DownloadAsync(bucketName, objectName);

            return File(stream, "application/octet-stream", objectName);
        }
    }
}