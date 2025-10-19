namespace Shared.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public UserDto User { get; set; } = null!;
    public IEnumerable<CommentReactionDto>? Reactions { get; set; }
    public IEnumerable<CommentAttachmentDto>? Attachments { get; set; }
    public IEnumerable<CommentMentionDto>? Mentions { get; set; }
}

public class CommentReactionDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class CommentAttachmentDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CommentMentionDto
{
    public Guid Id { get; set; }
    public Guid CommentId { get; set; }
    public Guid MentionedUserId { get; set; }
    public int StartIndex { get; set; }
    public int Length { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserDto MentionedUser { get; set; } = null!;
}