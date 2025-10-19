using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Board.Queries;

namespace Application.Board.Handlers;

internal class GetBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetBoardQuery, BoardDto?>
{
    public async Task<BoardDto?> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var board = await _boardRepository.GetByIdWithDetailsAsync(request.Id);
        if (board == null)
        {
            return null;
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have access to this board");
        }

        return new BoardDto
        {
            Id = board.Id,
            Title = board.Title,
            Description = board.Description,
            Logo = board.Logo,
            WorkspaceId = board.WorkspaceId,
            WorkspaceName = board.Workspace?.Name ?? "Unknown",
            CreatedBy = board.CreatedBy,
            CreatorName = board.Creator?.Name ?? "Unknown",
            CreatedAt = board.CreatedAt,
            ListCount = board.Lists?.Count() ?? 0,
            CardCount = board.Lists?.SelectMany(l => l.Cards).Count() ?? 0,
            IsPrivate = board.IsPrivate,
            BoardMembers = board.BoardMembers?.Select(bm => new MemberDto
            {
                Id = bm.Id,
                UserId = bm.UserId,
                WorkspaceId = board.WorkspaceId,
                UserName = bm.User?.Name ?? "Unknown",
                UserEmail = bm.User?.Email ?? "Unknown",
                UserAvatar = bm.User?.Avatar,
                Role = Domain.Enums.WorkspaceRole.Member,
                JoinedAt = bm.AssignedAt
            }),
            Lists = board.Lists?.Select(l => new ListDto
            {
                Id = l.Id,
                Title = l.Title,
                Position = l.Position,
                BoardId = l.BoardId,
                BoardTitle = board.Title,
                CreatedAt = l.CreatedAt,                  
                CardCount = l.Cards?.Count() ?? 0,
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
                    IsDeleted = c.IsDeleted,
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
                        AssignedByName = cm.AssignedByUser?.Name ?? "Unknown",
                    })
                })
            })
        };
    }
}