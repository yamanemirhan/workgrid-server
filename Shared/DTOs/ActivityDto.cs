using Domain.Enums;

namespace Shared.DTOs;

public class ActivityDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? BoardId { get; set; }
    public Guid? ListId { get; set; }
    public Guid? CardId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public ActivityType Type { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string? Metadata { get; set; }
    public string? WorkspaceName { get; set; }
    public string? BoardName { get; set; }
    public string? ListName { get; set; }
    public string? CardName { get; set; }
}