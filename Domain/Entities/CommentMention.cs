namespace Domain.Entities;

public class CommentMention : BaseEntity
{
    public Guid CommentId { get; set; }
    public Guid MentionedUserId { get; set; }
    public int StartIndex { get; set; }
    public int Length { get; set; }

    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User MentionedUser { get; set; } = null!;
}