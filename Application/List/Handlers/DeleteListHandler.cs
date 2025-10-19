using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.List.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using Domain.Enums;

namespace Application.List.Handlers;

internal class DeleteListHandler(IListRepository _listRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<DeleteListCommand, bool>
{
    public async Task<bool> Handle(DeleteListCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var list = await _listRepository.GetByIdWithDetailsAsync(request.Id);
        if (list == null)
        {
            throw new ArgumentException("List not found");
        }

        var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, list.Board.WorkspaceId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this list");
        }

        // - Owners/Admins can delete any list
        // - Members can only delete lists they created themselves
        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(list.Board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role == WorkspaceRole.Member && list.CreatedBy != userId)
        {
            throw new UnauthorizedAccessException("Members can only delete lists they created themselves");
        }

        await _listRepository.DeleteAsync(request.Id);

        var listDeletedEvent = new ListDeletedEvent
        {
            ListId = list.Id,
            BoardId = list.BoardId,
            WorkspaceId = list.Board.WorkspaceId,
            UserId = userId,
            ListTitle = list.Title,
            Description = $"List deleted: {list.Title}",
            Metadata = null
        };
        await _rabbitMqPublisher.PublishAsync(listDeletedEvent);

        return true;
    }
}