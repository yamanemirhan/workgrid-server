using Domain.Entities;
using Domain.Enums;
using Shared.DTOs;

namespace NotificationService.Services;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? data = null,
        Guid? workspaceId = null,
        Guid? boardId = null,
        Guid? listId = null,
        Guid? cardId = null,
        Guid? relatedUserId = null);

    Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);
    Task SendNotificationToWorkspaceAsync(Guid workspaceId, NotificationDto notification, Guid? excludeUserId = null);
    Task SendNotificationToBoardAsync(Guid boardId, NotificationDto notification, Guid? excludeUserId = null);
    Task<int> GetUnreadCountAsync(Guid userId);
}