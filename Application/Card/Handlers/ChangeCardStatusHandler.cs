using Application.Card.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Application.Card.Handlers;

internal class ChangeCardStatusHandler(ICardRepository _cardRepository,
        ICardStatusRepository _cardStatusRepository,
        IListRepository _listRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<ChangeCardStatusCommand, bool>
{
    public async Task<bool> Handle(ChangeCardStatusCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.CardId);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var targetStatus = await _cardStatusRepository.GetByIdAsync(request.StatusId);
        if (targetStatus == null)
        {
            throw new ArgumentException("Status not found");
        }

        if (targetStatus.WorkspaceId.HasValue && targetStatus.WorkspaceId != card.List.Board.WorkspaceId)
        {
            throw new ArgumentException("Status does not belong to this workspace");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        bool canChangeStatus = false;

        if (currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin)
        {
            // Owners and admins can change the status of any card
            canChangeStatus = true;
        }
        else if (currentUserMember.Role == WorkspaceRole.Member)
        {
            // Members can only change the status of cards assigned to them
            var isAssignedToCard = await _cardRepository.IsUserCardMemberAsync(request.CardId, userId);
            canChangeStatus = isAssignedToCard;
        }

        if (!canChangeStatus)
        {
            throw new UnauthorizedAccessException("You don't have permission to change this card's status. Members can only change status of cards assigned to them.");
        }

        var oldStatus = card.Status;
        var oldListId = card.ListId;

        var boardLists = await _listRepository.GetBoardListsAsync(card.List.BoardId);
        var targetList = boardLists.FirstOrDefault(l => l.StatusId == request.StatusId);

        if (targetList == null)
        {
            throw new ArgumentException("No list found for the target status");
        }

        card.StatusId = request.StatusId;
        card.ListId = targetList.Id;
        
        if (oldListId != targetList.Id)
        {
            var nextPosition = await _cardRepository.GetNextPositionAsync(targetList.Id);
            card.Position = nextPosition;
        }
        
        card.UpdatedAt = DateTime.UtcNow;

        var updatedCard = await _cardRepository.UpdateAsync(card);

        if (oldListId != targetList.Id)
        {
            var cardMovedEvent = new CardMovedEvent
            {
                UserId = userId,
                WorkspaceId = card.List.Board.WorkspaceId,
                BoardId = card.List.BoardId,
                CardId = request.CardId,
                CardTitle = card.Title,
                OldListId = oldListId,
                NewListId = targetList.Id,
                Description = $"Card moved from one list to another due to status change",
                ActivityType = ActivityType.CardMoved,
                Metadata = JsonSerializer.Serialize(new { 
                    OldListId = oldListId,
                    NewListId = targetList.Id,
                    CardTitle = card.Title,
                    Reason = "StatusChange"
                })
            };
            await _rabbitMqPublisher.PublishAsync(cardMovedEvent);
        }

        var statusChangedEvent = new CardStatusChangedEvent
        {
            UserId = userId,
            WorkspaceId = card.List.Board.WorkspaceId,
            BoardId = card.List.BoardId,
            ListId = card.ListId,
            CardId = request.CardId,
            CardTitle = card.Title,
            OldStatusId = oldStatus?.Id,
            OldStatusName = oldStatus?.Name ?? "No Status",
            NewStatusId = request.StatusId,
            NewStatusName = targetStatus.Name,
            Description = $"Card status changed from '{oldStatus?.Name ?? "No Status"}' to '{targetStatus.Name}'",
            ActivityType = ActivityType.CardStatusChanged,
            Metadata = JsonSerializer.Serialize(new { 
                OldStatusId = oldStatus?.Id,
                OldStatusName = oldStatus?.Name ?? "No Status",
                NewStatusId = request.StatusId,
                NewStatusName = targetStatus.Name,
                CardTitle = card.Title
            })
        };

        await _rabbitMqPublisher.PublishAsync(statusChangedEvent);

        return updatedCard != null;
    }
}