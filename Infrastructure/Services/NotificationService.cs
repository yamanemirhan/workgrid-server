using Domain.Entities;
using Domain.Enums;
using Infrastructure.Hubs;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.SignalR;
using Shared.DTOs;

namespace Infrastructure.Services;

public class NotificationService(INotificationRepository _notificationRepository,
        IHubContext<NotificationHub> _hubContext) : INotificationService
{
    public async Task<Notification> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        string? data = null,
        Guid? workspaceId = null,
        Guid? boardId = null,
        Guid? listId = null,
        Guid? cardId = null,
        Guid? relatedUserId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Data = data,
            WorkspaceId = workspaceId,
            BoardId = boardId,
            ListId = listId,
            CardId = cardId,
            RelatedUserId = relatedUserId,
            CreatedAt = DateTime.UtcNow
        };

        var createdNotification = await _notificationRepository.CreateAsync(notification);

        // Create DTO for SignalR
        var notificationDto = new NotificationDto
        {
            Id = createdNotification.Id,
            UserId = createdNotification.UserId,
            Type = createdNotification.Type,
            Title = createdNotification.Title,
            Message = createdNotification.Message,
            Data = createdNotification.Data,
            IsRead = createdNotification.IsRead,
            CreatedAt = createdNotification.CreatedAt,
            WorkspaceId = createdNotification.WorkspaceId,
            BoardId = createdNotification.BoardId,
            ListId = createdNotification.ListId,
            CardId = createdNotification.CardId,
            RelatedUserId = createdNotification.RelatedUserId
        };

        // Send real-time notification
        await SendNotificationToUserAsync(userId, notificationDto);

        return createdNotification;
    }

    public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToWorkspaceAsync(Guid workspaceId, NotificationDto notification, Guid? excludeUserId = null)
    {
        var clients = _hubContext.Clients.Group($"workspace_{workspaceId}");

        if (excludeUserId.HasValue)
        {
            clients = _hubContext.Clients.GroupExcept($"workspace_{workspaceId}", $"user_{excludeUserId}");
        }

        await clients.SendAsync("ReceiveNotification", notification);
    }

    public async Task SendNotificationToBoardAsync(Guid boardId, NotificationDto notification, Guid? excludeUserId = null)
    {
        var clients = _hubContext.Clients.Group($"board_{boardId}");

        if (excludeUserId.HasValue)
        {
            clients = _hubContext.Clients.GroupExcept($"board_{boardId}", $"user_{excludeUserId}");
        }

        await clients.SendAsync("ReceiveNotification", notification);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _notificationRepository.GetUnreadNotificationCountAsync(userId);
    }
}