using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;
    private readonly string _region;

    public S3FileStorageService(
        IAmazonS3 s3Client, 
        IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = configuration["AWS:S3:BucketName"] ?? throw new ArgumentNullException("AWS:S3:BucketName configuration is required");
        _region = configuration["AWS:S3:Region"] ?? "us-east-1";
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "attachments")
    {
        try
        {
            var fileKey = GenerateFileKey(fileName, folder);
            
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.PublicRead,
                Metadata =
                {
                    ["original-filename"] = fileName,
                    ["upload-timestamp"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(request);
            
            return GetFileUrlFromKey(fileKey);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "attachments")
    {
        using var stream = new MemoryStream(fileBytes);
        return await UploadFileAsync(stream, fileName, contentType, folder);
    }

    public async Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fileKey = GetFileKeyFromUrl(fileUrl);
            
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = fileKey
            };

            await _s3Client.DeleteObjectAsync(request);
            
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string fileKey, TimeSpan expiration)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = fileKey,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.Add(expiration)
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public string GetFileUrlFromKey(string fileKey)
    {
        return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{fileKey}";
    }

    public string GetFileKeyFromUrl(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        return uri.AbsolutePath.TrimStart('/');
    }

    public async Task<string> GenerateThumbnailAsync(Stream imageStream, string originalFileName)
    {
        try
        {
            if (!IsImageFile(Path.GetExtension(originalFileName)))
            {
                return string.Empty;
            }

            using var image = await Image.LoadAsync(imageStream);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(200, 200),
                Mode = ResizeMode.Max
            }));

            var thumbnailStream = new MemoryStream();
            await image.SaveAsJpegAsync(thumbnailStream, new JpegEncoder { Quality = 80 });
            thumbnailStream.Position = 0;

            var thumbnailFileName = $"thumb_{originalFileName}";
            var thumbnailUrl = await UploadFileAsync(thumbnailStream, thumbnailFileName, "image/jpeg", "thumbnails");

            thumbnailStream.Dispose();
            return thumbnailUrl;
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    public async Task<long> GetFileSizeAsync(Stream fileStream)
    {
        return fileStream.Length;
    }

    public bool IsImageFile(string contentTypeOrExtension)
    {
        // Check if it's a content type (starts with "image/")
        if (contentTypeOrExtension.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check if it's a file extension
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        return imageExtensions.Contains(contentTypeOrExtension.ToLowerInvariant());
    }

    public string GetFileExtension(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant();
    }

    private string GenerateFileKey(string fileName, string folder)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // First 8 characters
        var sanitizedFileName = SanitizeFileName(fileName);
        
        return $"{folder}/{timestamp}/{uniqueId}_{sanitizedFileName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        // Remove or replace invalid characters
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        
        // Limit length and ensure it's not empty
        if (string.IsNullOrWhiteSpace(sanitized))
            sanitized = "file";
            
        return sanitized[..Math.Min(sanitized.Length, 100)]; // Max 100 characters
    }
}