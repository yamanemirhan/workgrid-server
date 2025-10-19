using Domain.Enums;

namespace Domain.Entities;

public class Activity : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ListId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? UserId { get; set; }
    public ActivityType Type { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? Metadata { get; set; }

    // Navigation
    public Workspace Workspace { get; set; } = null!;
    public Board? Board { get; set; }
    public List? List { get; set; }
    public Card? Card { get; set; }
    public User? User { get; set; }
}