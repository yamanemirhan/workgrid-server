using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Activity.Queries;
using Shared.DTOs;

namespace Application.Activity.Handlers;

public class GetWorkspaceActivitiesHandler : IRequestHandler<GetWorkspaceActivitiesQuery, IEnumerable<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GetWorkspaceActivitiesHandler(
        IActivityRepository activityRepository,
        IWorkspaceRepository workspaceRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _activityRepository = activityRepository;
        _workspaceRepository = workspaceRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<ActivityDto>> Handle(GetWorkspaceActivitiesQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You are not a member of this workspace");
        }

        var activities = await _activityRepository.GetActivitiesByWorkspaceIdAsync(request.WorkspaceId, request.Take);

        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            WorkspaceId = a.WorkspaceId,
            BoardId = a.BoardId,
            ListId = a.ListId,
            CardId = a.CardId,
            UserId = a.UserId,
            UserName = a.User?.Name,
            UserAvatar = a.User?.Avatar,
            Type = a.Type,
            Description = a.Description,
            CreatedAt = a.CreatedAt,
            EntityId = a.EntityId,
            EntityType = a.EntityType,
            Metadata = a.Metadata,
            WorkspaceName = a.Workspace?.Name,
            BoardName = a.Board?.Title,
            ListName = a.List?.Title,
            CardName = a.Card?.Title
        });
    }
}

public class GetBoardActivitiesHandler : IRequestHandler<GetBoardActivitiesQuery, IEnumerable<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetBoardActivitiesHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityDto>> Handle(GetBoardActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.GetActivitiesByBoardIdAsync(request.BoardId, request.Take);
        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            WorkspaceId = a.WorkspaceId,
            BoardId = a.BoardId,
            ListId = a.ListId,
            CardId = a.CardId,
            UserId = a.UserId,
            UserName = a.User?.Name,
            UserAvatar = a.User?.Avatar,
            Type = a.Type,
            Description = a.Description,
            CreatedAt = a.CreatedAt,
            EntityId = a.EntityId,
            EntityType = a.EntityType,
            Metadata = a.Metadata,
            BoardName = a.Board?.Title,
            ListName = a.List?.Title,
            CardName = a.Card?.Title
        });
    }
}

public class GetCardActivitiesHandler : IRequestHandler<GetCardActivitiesQuery, IEnumerable<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetCardActivitiesHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityDto>> Handle(GetCardActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.GetActivitiesByCardIdAsync(request.CardId, request.Take);
        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            WorkspaceId = a.WorkspaceId,
            BoardId = a.BoardId,
            ListId = a.ListId,
            CardId = a.CardId,
            UserId = a.UserId,
            UserName = a.User?.Name,
            UserAvatar = a.User?.Avatar,
            Type = a.Type,
            Description = a.Description,
            CreatedAt = a.CreatedAt,
            EntityId = a.EntityId,
            EntityType = a.EntityType,
            Metadata = a.Metadata,
            CardName = a.Card?.Title
        });
    }
}

public class GetListActivitiesHandler : IRequestHandler<GetListActivitiesQuery, IEnumerable<ActivityDto>>
{
    private readonly IActivityRepository _activityRepository;

    public GetListActivitiesHandler(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<IEnumerable<ActivityDto>> Handle(GetListActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _activityRepository.GetActivitiesByListIdAsync(request.ListId, request.Take);
        return activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            WorkspaceId = a.WorkspaceId,
            BoardId = a.BoardId,
            ListId = a.ListId,
            CardId = a.CardId,
            UserId = a.UserId,
            UserName = a.User?.Name,
            UserAvatar = a.User?.Avatar,
            Type = a.Type,
            Description = a.Description,
            CreatedAt = a.CreatedAt,
            EntityId = a.EntityId,
            EntityType = a.EntityType,
            Metadata = a.Metadata,
            ListName = a.List?.Title,
            CardName = a.Card?.Title
        });
    }
}