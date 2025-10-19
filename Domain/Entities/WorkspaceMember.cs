using Domain.Enums;

namespace Domain.Entities;

public class WorkspaceMember : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Workspace Workspace { get; set; } = null!;
}