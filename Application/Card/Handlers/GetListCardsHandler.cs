using Application.Card.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Card.Handlers;

internal class GetListCardsHandler(ICardRepository _cardRepository,
        IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetListCardsQuery, IEnumerable<CardDto>>
{
    public async Task<IEnumerable<CardDto>> Handle(GetListCardsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var list = await _listRepository.GetByIdWithDetailsAsync(request.ListId);
        if (list == null)
        {
            throw new ArgumentException("List not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, list.Board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to view cards in this list");
        }

        var cards = await _cardRepository.GetListCardsAsync(request.ListId);

        return cards.Select(c => new CardDto
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            Position = c.Position,
            ListId = c.ListId,
            ListTitle = c.List?.Title ?? "Unknown",
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
                AssignedAt = cm.AssignedAt,
                AssignedBy = cm.AssignedBy,
                AssignedByName = cm.AssignedByUser?.Name ?? "Unknown"
            }),
            CardFollowers = c.CardFollowers?.Select(cf => new CardFollowerDto
            {
                Id = cf.Id,
                CardId = cf.CardId,
                UserId = cf.UserId,
                UserName = cf.User?.Name ?? "Unknown",
                UserEmail = cf.User?.Email ?? "Unknown",
                UserAvatar = cf.User?.Avatar,
                FollowedAt = cf.FollowedAt
            })
        }).OrderBy(c => c.Position);
    }
}
