using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NotificationService.Repositories;
using NotificationService.Services;
using Shared.DTOs;
using Shared.Responses;

namespace NotificationService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationController(INotificationRepository _notificationRepository,
        INotificationService _notificationService,
        IHttpContextAccessor _httpContextAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUserNotifications(int take = 50, int skip = 0)
    {
        try
        {
            // Get current user from JWT token
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(ResponseHelper.Unauthorized("User not authenticated"));
            }

            var notifications = await _notificationRepository.GetUserNotificationsAsync(currentUserId, take, skip);
            
            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                UserId = n.UserId,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                Data = n.Data,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt,
                WorkspaceId = n.WorkspaceId,
                WorkspaceName = n.Workspace?.Name,
                BoardId = n.BoardId,
                BoardTitle = n.Board?.Title,
                ListId = n.ListId,
                ListTitle = n.List?.Title,
                CardId = n.CardId,
                CardTitle = n.Card?.Title,
                RelatedUserId = n.RelatedUserId,
                RelatedUserName = n.RelatedUser?.Name,
                RelatedUserAvatar = n.RelatedUser?.Avatar
            });

            return Ok(ResponseHelper.Success(notificationDtos, "Notifications retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving notifications"));
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadNotificationCount()
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(ResponseHelper.Unauthorized("User not authenticated"));
            }

            var count = await _notificationService.GetUnreadCountAsync(currentUserId);
            return Ok(ResponseHelper.Success(new { count }, "Unread notification count retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving unread notification count"));
        }
    }

    [HttpPut("{notificationId}/mark-as-read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(ResponseHelper.Unauthorized("User not authenticated"));
            }

            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return NotFound(ResponseHelper.Error("Notification not found"));
            }

            if (notification.UserId != currentUserId)
            {
                return StatusCode(403, ResponseHelper.Error("You can only mark your own notifications as read"));
            }

            await _notificationRepository.MarkAsReadAsync(notificationId);
            return Ok(ResponseHelper.Success(true, "Notification marked as read"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while marking notification as read"));
        }
    }

    [HttpPut("mark-all-as-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(ResponseHelper.Unauthorized("User not authenticated"));
            }

            var count = await _notificationRepository.MarkAllAsReadAsync(currentUserId);
            return Ok(ResponseHelper.Success(new { markedCount = count }, $"{count} notifications marked as read"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while marking all notifications as read"));
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(ResponseHelper.Unauthorized("User not authenticated"));
            }

            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                return NotFound(ResponseHelper.Error("Notification not found"));
            }

            if (notification.UserId != currentUserId)
            {
                return StatusCode(403, ResponseHelper.Error("You can only delete your own notifications"));
            }

            await _notificationRepository.DeleteAsync(notificationId);
            return Ok(ResponseHelper.Success(true, "Notification deleted successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while deleting notification"));
        }
    }
}