using Domain.Enums;

namespace Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; } 
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    
    public Guid? WorkspaceId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ListId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? RelatedUserId { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? RelatedUser { get; set; }
    public Workspace? Workspace { get; set; }
    public Board? Board { get; set; }
    public List? List { get; set; }
    public Card? Card { get; set; }
}