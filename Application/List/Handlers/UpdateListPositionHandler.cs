using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.List.Commands;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using Domain.Enums;

namespace Application.List.Handlers;

internal class UpdateListPositionHandler(IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UpdateListPositionCommand, ListDto>
{
    public async Task<ListDto> Handle(UpdateListPositionCommand request, CancellationToken cancellationToken)
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
            throw new UnauthorizedAccessException("You don't have permission to update this list");
        }

        // Check permissions: Owners/Admins can reorder any list, Members can only reorder lists they created
        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(list.Board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role == WorkspaceRole.Member && list.CreatedBy != userId)
        {
            throw new UnauthorizedAccessException("Members can only reorder lists they created themselves");
        }

        var oldPosition = list.Position;
        list.Position = request.Position;
        list.UpdatedAt = DateTime.UtcNow;

        var updatedList = await _listRepository.UpdateAsync(list);

        // Publish list position updated event
        var listUpdatedEvent = new ListUpdatedEvent
        {
            ListId = updatedList.Id,
            BoardId = updatedList.BoardId,
            WorkspaceId = list.Board.WorkspaceId,
            UserId = userId,
            ListTitle = updatedList.Title,
            Description = $"List position updated from {oldPosition} to {request.Position}",
            Metadata = null,
            ActivityType = ActivityType.ListUpdated
        };
        await _rabbitMqPublisher.PublishAsync(listUpdatedEvent);

        var cardCount = await _listRepository.GetCardCountByListIdAsync(updatedList.Id);

        return new ListDto
        {
            Id = updatedList.Id,
            Title = updatedList.Title,
            Position = updatedList.Position,
            BoardId = updatedList.BoardId,
            BoardTitle = list.Board?.Title ?? "Unknown",
            CreatedAt = updatedList.CreatedAt,
            CardCount = cardCount
        };
    }
}
