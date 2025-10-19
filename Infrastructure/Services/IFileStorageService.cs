namespace Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string folder = "attachments");
    Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string contentType, string folder = "attachments");
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<string> GeneratePresignedUrlAsync(string fileKey, TimeSpan expiration);
    string GetFileUrlFromKey(string fileKey);
    string GetFileKeyFromUrl(string fileUrl);
    Task<string> GenerateThumbnailAsync(Stream imageStream, string originalFileName);
    Task<long> GetFileSizeAsync(Stream fileStream);
    bool IsImageFile(string contentType);
    string GetFileExtension(string fileName);
}