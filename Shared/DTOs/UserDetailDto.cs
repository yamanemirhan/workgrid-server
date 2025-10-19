using Domain.Enums;

namespace Shared.DTOs;

public class UserDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Workspace memberships
    public ICollection<WorkspaceMembershipDto> WorkspaceMemberships { get; set; } = new List<WorkspaceMembershipDto>();
    
    // Owned workspaces
    public ICollection<WorkspaceDto> OwnedWorkspaces { get; set; } = new List<WorkspaceDto>();
    public int TotalBoardsCount { get; set; }
    public int TotalCardsCount { get; set; }
}

public class WorkspaceMembershipDto
{
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string? WorkspaceLogo { get; set; }
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
}