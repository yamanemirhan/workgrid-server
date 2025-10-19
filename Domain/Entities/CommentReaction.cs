namespace Domain.Entities;

public class CommentReaction : BaseEntity
{
    public Guid CommentId { get; set; }
    public Guid UserId { get; set; }
    public string Emoji { get; set; } = string.Empty;
    
    // Navigation properties
    public Comment Comment { get; set; } = null!;
    public User User { get; set; } = null!;
}