using Domain.Enums;

namespace Shared.DTOs;

public class MemberDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkspaceId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public WorkspaceRole Role { get; set; }
    public string RoleDisplayName => Role switch
    {
        WorkspaceRole.Owner => "Workspace Owner",
        WorkspaceRole.Admin => "Administrator",
        WorkspaceRole.Member => "Member",
        _ => "Unknown Role"
    };
    public DateTime JoinedAt { get; set; }
    public bool IsOwner => Role == WorkspaceRole.Owner;
    public bool IsAdmin => Role == WorkspaceRole.Admin || Role == WorkspaceRole.Owner;
    public bool CanManageMembers => Role == WorkspaceRole.Owner || Role == WorkspaceRole.Admin;
    public bool CanManageWorkspace => Role == WorkspaceRole.Owner;
}
