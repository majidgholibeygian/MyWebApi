using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using mywebapi.Models;
using mywebapi.Services;

namespace mywebapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly IStorageService _storage;
        private readonly MinioOptions _options;

        public StorageController(IStorageService storage, IOptions<MinioOptions> options)
        {
            _storage = storage;
            _options = options.Value;
        }

        /// <summary>
        /// Upload a file to MinIO. Form-data: file (IFormFile). Optional query: bucket.
        /// </summary>
        [HttpPost("upload")]     
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] string? bucket)
        {
            if (file == null) return BadRequest("file is required");

            var bucketName = string.IsNullOrWhiteSpace(bucket) ? _options.BucketName : bucket;
            var objectName = file.FileName;

            await _storage.UploadAsync(file, bucketName, objectName);
            return Ok(new { bucket = bucketName, objectName });
        }

        /// <summary>
        /// Download a file from MinIO. Query: objectName, optional bucket.
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> Download([FromQuery] string objectName, [FromQuery] string? bucket)
        {
            if (string.IsNullOrWhiteSpace(objectName)) return BadRequest("objectName is required");

            var bucketName = string.IsNullOrWhiteSpace(bucket) ? _options.BucketName : bucket;
            var stream = await _storage.DownloadAsync(bucketName, objectName);

            return File(stream, "application/octet-stream", objectName);
        }
    }
}