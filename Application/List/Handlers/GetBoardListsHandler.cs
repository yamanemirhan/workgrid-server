using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.List.Queries;

namespace Application.List.Handlers;

internal class GetBoardListsHandler(IListRepository _listRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetBoardListsQuery, IEnumerable<ListDto>>
{
    public async Task<IEnumerable<ListDto>> Handle(GetBoardListsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var board = await _boardRepository.GetByIdAsync(request.BoardId);
        if (board == null)
        {
            throw new ArgumentException("Board not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to view lists in this board");
        }

        var lists = await _listRepository.GetBoardListsAsync(request.BoardId);

        return lists.Select(l => new ListDto
        {
            Id = l.Id,
            Title = l.Title,
            Position = l.Position,
            BoardId = l.BoardId,
            BoardTitle = l.Board?.Title ?? "Unknown",
            CreatedAt = l.CreatedAt,
            CardCount = l.Cards?.Count ?? 0,
            IsDeleted = l.IsDeleted,
            Cards = l.Cards?.Select(c => new CardDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Position = c.Position,
                ListId = c.ListId,
                ListTitle = l.Title,
                CreatedBy = c.CreatedBy,
                CreatorName = c.Creator?.Name ?? "Unknown",
                Creator = c.Creator != null ? new UserDto
                {
                    Id = c.Creator.Id,
                    Name = c.Creator.Name,
                    Email = c.Creator.Email,
                    Avatar = c.Creator.Avatar,
                    CreatedAt = c.Creator.CreatedAt
                } : null,
                EstimatedTime = c.EstimatedTime,
                Tags = c.Tags,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                StatusId = c.StatusId,
                Status = c.Status != null ? new CardStatusDto
                {
                    Id = c.Status.Id,
                    Name = c.Status.Name,
                    Color = c.Status.Color,
                    Position = c.Status.Position,
                    IsDefault = c.Status.IsDefault,
                    Description = c.Status.Description,
                    Type = c.Status.Type,
                    UpdatedAt = c.Status.UpdatedAt,
                    CreatedAt = c.Status.CreatedAt,
                    WorkspaceId = c.Status.WorkspaceId,
                    WorkspaceName = c.Status.Workspace?.Name,
                } : null,
                CardMembers = c.CardMembers?.Select(cm => new CardMemberDto
                {
                    Id = cm.Id,
                    CardId = cm.CardId,
                    UserId = cm.UserId,
                    UserName = cm.User?.Name ?? "Unknown",
                    UserEmail = cm.User?.Email ?? "Unknown",
                    UserAvatar = cm.User?.Avatar,
                }),
            })
        });
    }
}
                   