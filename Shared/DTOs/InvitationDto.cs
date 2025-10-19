using Domain.Enums;

namespace Shared.DTOs;

public class InvitationDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public WorkspaceRole Role { get; set; }
    public Guid InvitedByUserId { get; set; }
    public string InvitedByUserName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}


