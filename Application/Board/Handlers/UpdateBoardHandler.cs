using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Board.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using Domain.Enums;

namespace Application.Board.Handlers;

internal class UpdateBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UpdateBoardCommand, BoardDto>
{
    public async Task<BoardDto> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var board = await _boardRepository.GetByIdWithDetailsAsync(request.Id);
        if (board == null)
        {
            throw new ArgumentException("Board not found");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);
        
        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role == WorkspaceRole.Member)
        {
            throw new UnauthorizedAccessException("Members cannot edit boards. Only workspace owners and admins can update boards");
        }

        bool canUpdate = currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin;
        bool canChangePrivacy = currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin;

        if (!canUpdate)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this board");
        }

        if (request.IsPrivate != board.IsPrivate && !canChangePrivacy)
        {
            throw new UnauthorizedAccessException("You don't have permission to change board privacy");
        }

        board.Title = request.Title;
        board.Description = request.Description;
        board.Logo = request.Logo;
        board.IsPrivate = request.IsPrivate;
        board.UpdatedAt = DateTime.UtcNow;

        var updatedBoard = await _boardRepository.UpdateAsync(board);

        var boardUpdatedEvent = new BoardUpdatedEvent
        {
            BoardId = updatedBoard.Id,
            BoardTitle = updatedBoard.Title,
            WorkspaceId = updatedBoard.WorkspaceId,
            UserId = userId,
            Description = $"Board updated: {updatedBoard.Title}",
            Metadata = null,
            ActivityType = ActivityType.BoardUpdated
        };
        await _rabbitMqPublisher.PublishAsync(boardUpdatedEvent);

        var cardCount = await _boardRepository.GetCardCountByBoardIdAsync(updatedBoard.Id);

        return new BoardDto
        {
            Id = updatedBoard.Id,
            Title = updatedBoard.Title,
            Description = updatedBoard.Description,
            Logo = updatedBoard.Logo,
            WorkspaceId = updatedBoard.WorkspaceId,
            WorkspaceName = board.Workspace?.Name ?? "Unknown",
            CreatedBy = updatedBoard.CreatedBy,
            CreatorName = board.Creator?.Name ?? "Unknown",
            CreatedAt = updatedBoard.CreatedAt,
            ListCount = board.Lists?.Count() ?? 0,
            CardCount = cardCount,
            IsPrivate = updatedBoard.IsPrivate
        };
    }
}