using Domain.Enums;

namespace Shared.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Data { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    

    public Guid? WorkspaceId { get; set; }
    public string? WorkspaceName { get; set; }
    public Guid? BoardId { get; set; }
    public string? BoardTitle { get; set; }
    public Guid? ListId { get; set; }
    public string? ListTitle { get; set; }
    public Guid? CardId { get; set; }
    public string? CardTitle { get; set; }
    

    public Guid? RelatedUserId { get; set; }
    public string? RelatedUserName { get; set; }
    public string? RelatedUserAvatar { get; set; }
}