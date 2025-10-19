namespace Domain.Entities;

public class Card : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Position { get; set; }
    public Guid ListId { get; set; }
    public Guid CreatedBy { get; set; }
    public string? EstimatedTime { get; set; } // e.g., "10min", "1 hour", "2 days", etc.
    public string? Tags { get; set; } // Comma-separated tags
    public bool IsDeleted { get; set; }
    public Guid? StatusId { get; set; } // Card status - defaults to "To-Do" for new cards

    // Navigation properties
    public List List { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public CardStatus? Status { get; set; }
    public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public ICollection<CardMember> CardMembers { get; set; } = new List<CardMember>();
    public ICollection<CardFollower> CardFollowers { get; set; } = new List<CardFollower>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
