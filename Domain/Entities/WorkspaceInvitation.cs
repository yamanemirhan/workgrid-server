using Domain.Enums;

namespace Domain.Entities;

public class WorkspaceInvitation : BaseEntity
{
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    // Navigation properties
    public virtual Workspace? Workspace { get; set; }
    public virtual User? InvitedBy { get; set; }
}