using MediatR;
using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Board.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using System.Text.Json;

namespace Application.Board.Handlers;

internal class AssignUserToBoardHandler(IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<AssignUserToBoardCommand, MemberDto>
{
    public async Task<MemberDto> Handle(AssignUserToBoardCommand request, CancellationToken cancellationToken)
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

        // Check if user to assign exists and is workspace member
        var userToAssign = await _userRepository.GetByIdAsync(request.UserId);
        if (userToAssign == null)
        {
            throw new ArgumentException("User to assign not found");
        }

        // Get workspace members to check permissions
        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == currentUserId);
        var userToAssignMember = workspaceMembers.FirstOrDefault(m => m.UserId == request.UserId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (userToAssignMember == null)
        {
            throw new ArgumentException("User to assign is not a member of this workspace");
        }

        // Check permissions - only Owner/Admin can assign users to boards
        if (currentUserMember.Role != WorkspaceRole.Owner && currentUserMember.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace owners and admins can assign users to boards");
        }

        // Check if user is already assigned to board
        if (await _boardRepository.IsUserBoardMemberAsync(request.BoardId, request.UserId))
        {
            throw new InvalidOperationException("User is already assigned to this board");
        }

        var boardMember = new BoardMember
        {
            BoardId = request.BoardId,
            UserId = request.UserId,
            AssignedBy = currentUserId,
            AssignedAt = DateTime.UtcNow
        };

        await _boardRepository.AssignUserToBoardAsync(boardMember);

        var assignedEvent = new BoardMemberAssignedEvent
        {
            UserId = currentUserId,
            WorkspaceId = board.WorkspaceId,
            BoardId = request.BoardId,
            BoardTitle = board.Title,
            AssignedUserId = request.UserId,
            AssignedUserName = userToAssign.Name,
            AssignedUserEmail = userToAssign.Email,
            ActivityType = ActivityType.BoardMemberAssigned,
            Description = $"User {userToAssign.Name} was assigned to board {board.Title}",
            Metadata = JsonSerializer.Serialize(new { 
                AssignedUserId = request.UserId,
                AssignedUserName = userToAssign.Name,
                AssignedUserEmail = userToAssign.Email,
                BoardTitle = board.Title
            })
        };

        await _rabbitMqPublisher.PublishAsync(assignedEvent);

        return new MemberDto
        {
            Id = userToAssignMember.Id,
            UserId = userToAssignMember.UserId,
            WorkspaceId = userToAssignMember.WorkspaceId,
            UserName = userToAssign.Name,
            UserEmail = userToAssign.Email,
            UserAvatar = userToAssign.Avatar,
            Role = userToAssignMember.Role,
            JoinedAt = userToAssignMember.JoinedAt
        };
    }
}