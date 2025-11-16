using System;

namespace mywebapi.Models
{
    // Options for MinIO. Values can be overridden via configuration (appsettings.json / environment variables).
    public class MinioOptions
    {
        public string Endpoint { get; set; } = "localhost:9000";
        public string AccessKey { get; set; } = "minioadmin";
        public string SecretKey { get; set; } = "Str0ngP@ss";
        public bool Secure { get; set; } = false;
        public string BucketName { get; set; } = "uploads";
    }
}