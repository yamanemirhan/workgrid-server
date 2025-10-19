using Application.List.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.List.Handlers;

internal class GetListHandler(IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetListQuery, ListDto?>
{
    public async Task<ListDto?> Handle(GetListQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var list = await _listRepository.GetByIdWithDetailsAsync(request.Id);
        if (list == null)
        {
            return null;
        }

        // Check if user has access to this list (is workspace member)
        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, list.Board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have access to this list");
        }

        var cardCount = await _listRepository.GetCardCountByListIdAsync(list.Id);

        return new ListDto
        {
            Id = list.Id,
            Title = list.Title,
            Position = list.Position,
            BoardId = list.BoardId,
            BoardTitle = list.Board?.Title ?? "Unknown",
            CreatedAt = list.CreatedAt,
            CardCount = cardCount,
            Cards = list.Cards?.Select(c => new CardDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                Position = c.Position,
                ListId = c.ListId,
                ListTitle = list.Title,
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
        };
    }
}

