namespace Domain.Entities;

public class Comment : BaseEntity
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsEdited { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public DateTime? EditedAt { get; set; }

    // Navigation properties
    public Card Card { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<CommentAttachment> Attachments { get; set; } = new List<CommentAttachment>();
    public ICollection<CommentMention> Mentions { get; set; } = new List<CommentMention>();
    public ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
}