using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Board.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using System.Text.Json;

namespace Application.Board.Handlers;

internal class UnassignUserFromBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UnassignUserFromBoardCommand, bool>
{
    public async Task<bool> Handle(UnassignUserFromBoardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var board = await _boardRepository.GetByIdWithDetailsAsync(request.BoardId);
        if (board == null)
        {
            throw new ArgumentException("Board not found");
        }

        var userToUnassign = await _userRepository.GetByIdAsync(request.UserId);
        if (userToUnassign == null)
        {
            throw new ArgumentException("User to unassign not found");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == currentUserId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role != WorkspaceRole.Owner && currentUserMember.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace owners and admins can unassign users from boards");
        }

        if (!await _boardRepository.IsUserBoardMemberAsync(request.BoardId, request.UserId))
        {
            throw new InvalidOperationException("User is not assigned to this board");
        }

        var result = await _boardRepository.UnassignUserFromBoardAsync(request.BoardId, request.UserId);

        if (result)
        {
            var unassignedEvent = new BoardMemberUnassignedEvent
            {
                UserId = currentUserId,
                WorkspaceId = board.WorkspaceId,
                BoardId = request.BoardId,
                BoardTitle = board.Title,
                UnassignedUserId = request.UserId,
                UnassignedUserName = userToUnassign.Name,
                UnassignedUserEmail = userToUnassign.Email,
                Description = $"User {userToUnassign.Name} was unassigned from board {board.Title}",
                ActivityType = ActivityType.BoardMemberUnassigned,
                Metadata = JsonSerializer.Serialize(new { 
                    UnassignedUserId = request.UserId,
                    UnassignedUserName = userToUnassign.Name,
                    UnassignedUserEmail = userToUnassign.Email,
                    BoardTitle = board.Title
                })
            };

            await _rabbitMqPublisher.PublishAsync(unassignedEvent);
        }

        return result;
    }
}