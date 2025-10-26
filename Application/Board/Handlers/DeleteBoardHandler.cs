using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Board.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using Domain.Enums;

namespace Application.Board.Handlers;

internal class DeleteBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<DeleteBoardCommand, bool>
{
    public async Task<bool> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("Members cannot delete boards. Only workspace owners and admins can delete boards");
        }

        await _boardRepository.DeleteAsync(request.Id);

        var boardDeletedEvent = new BoardDeletedEvent
        {
            BoardId = board.Id,
            BoardTitle = board.Title,
            WorkspaceId = board.WorkspaceId,
            UserId = userId,
            Description = $"Board deleted: {board.Title}",
            Metadata = null,
            ActivityType = ActivityType.BoardDeleted,
        };
        await _rabbitMqPublisher.PublishAsync(boardDeletedEvent);

        return true;
    }
}