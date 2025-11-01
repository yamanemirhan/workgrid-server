using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ActivityService.Services;
using Shared.DTOs;
using Shared.Responses;

namespace ActivityService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivityController(IActivityService _activityService,
        IHttpContextAccessor _httpContextAccessor) : ControllerBase
{
    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceActivities(
        Guid workspaceId, 
        int take = 50, 
        int skip = 0)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByWorkspaceIdAsync(workspaceId, skip / take + 1, take);
            
            var activityDtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                WorkspaceId = a.WorkspaceId,
                BoardId = a.BoardId,
                ListId = a.ListId,
                CardId = a.CardId,
                UserId = a.UserId,
                Type = a.Type.ToString(),
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                EntityId = a.EntityId,
                EntityType = a.EntityType,
                Metadata = a.Metadata
            });

            return Ok(ResponseHelper.Success(activityDtos, "Workspace activities retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ResponseHelper.Unauthorized(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving workspace activities"));
        }
    }

    [HttpGet("board/{boardId}")]
    public async Task<IActionResult> GetBoardActivities(
        Guid boardId, 
        int take = 50, 
        int skip = 0)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByBoardIdAsync(boardId, skip / take + 1, take);
            
            var activityDtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                WorkspaceId = a.WorkspaceId,
                BoardId = a.BoardId,
                ListId = a.ListId,
                CardId = a.CardId,
                UserId = a.UserId,
                Type = a.Type.ToString(),
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                EntityId = a.EntityId,
                EntityType = a.EntityType,
                Metadata = a.Metadata
            });

            return Ok(ResponseHelper.Success(activityDtos, "Board activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving board activities"));
        }
    }

    [HttpGet("list/{listId}")]
    public async Task<IActionResult> GetListActivities(
        Guid listId, 
        int take = 50, 
        int skip = 0)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByListIdAsync(listId, skip / take + 1, take);
            
            var activityDtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                WorkspaceId = a.WorkspaceId,
                BoardId = a.BoardId,
                ListId = a.ListId,
                CardId = a.CardId,
                UserId = a.UserId,
                Type = a.Type.ToString(),
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                EntityId = a.EntityId,
                EntityType = a.EntityType,
                Metadata = a.Metadata
            });

            return Ok(ResponseHelper.Success(activityDtos, "List activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving list activities"));
        }
    }

    [HttpGet("card/{cardId}")]
    public async Task<IActionResult> GetCardActivities(
        Guid cardId, 
        int take = 50, 
        int skip = 0)
    {
        try
        {
            var activities = await _activityService.GetActivitiesByCardIdAsync(cardId, skip / take + 1, take);
            
            var activityDtos = activities.Select(a => new ActivityDto
            {
                Id = a.Id,
                WorkspaceId = a.WorkspaceId,
                BoardId = a.BoardId,
                ListId = a.ListId,
                CardId = a.CardId,
                UserId = a.UserId,
                Type = a.Type.ToString(),
                Description = a.Description,
                CreatedAt = a.CreatedAt,
                EntityId = a.EntityId,
                EntityType = a.EntityType,
                Metadata = a.Metadata
            });

            return Ok(ResponseHelper.Success(activityDtos, "Card activities retrieved successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ResponseHelper.Error("An error occurred while retrieving card activities"));
        }
    }
}