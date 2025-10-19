namespace Domain.Entities;

public class CommentAttachment : BaseEntity
{
    public Guid CommentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty; // S3 URL
    public string FileType { get; set; } = string.Empty; // image/pdf/document etc.
    public long FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Navigation properties
    public Comment Comment { get; set; } = null!;
}