using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Board.Queries;
using Domain.Enums;

namespace Application.Board.Handlers;

internal class GetWorkspaceBoardsHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetWorkspaceBoardsQuery, IEnumerable<BoardDto>>
{
    public async Task<IEnumerable<BoardDto>> Handle(GetWorkspaceBoardsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var userRole = await _workspaceRepository.GetUserRoleInWorkspaceAsync(userId, request.WorkspaceId);
        if (userRole == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to view boards in this workspace");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);

        // If user is Owner or Admin, show all boards (including private ones)
        // If user is Member, use GetWorkspaceBoardsForUserAsync to respect privacy settings
        IEnumerable<Domain.Entities.Board> boards;
        
        if (userRole == WorkspaceRole.Owner || userRole == WorkspaceRole.Admin)
        {
            boards = await _boardRepository.GetWorkspaceBoardsAsync(request.WorkspaceId);
        }
        else
        {
            boards = await _boardRepository.GetWorkspaceBoardsForUserAsync(request.WorkspaceId, userId);
        }

        return boards.Select(b => new BoardDto
        {
            Id = b.Id,
            Title = b.Title,
            Description = b.Description,
            Logo = b.Logo,
            WorkspaceId = b.WorkspaceId,
            WorkspaceName = workspace?.Name ?? "Unknown",
            CreatedBy = b.CreatedBy,
            CreatorName = b.Creator?.Name ?? "Unknown",
            CreatedAt = b.CreatedAt,
            ListCount = b.Lists?.Count ?? 0,
            CardCount = b.Lists?.SelectMany(l => l.Cards).Count() ?? 0,
            IsPrivate = b.IsPrivate,
            BoardMembers = b.BoardMembers?.Select(bm => new MemberDto
            {
                Id = bm.Id,
                UserId = bm.UserId,
                WorkspaceId = b.WorkspaceId,
                UserName = bm.User?.Name ?? "Unknown",
                UserEmail = bm.User?.Email ?? "Unknown",
                UserAvatar = bm.User?.Avatar,
                JoinedAt = bm.AssignedAt
            })
        });
    }
}