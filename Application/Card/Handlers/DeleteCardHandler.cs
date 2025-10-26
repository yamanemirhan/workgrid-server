using Application.Card.Commands;
using Domain.Enums;
using Domain.Events;
using Infrastructure.Messaging.RabbitMQ;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Card.Handlers;

internal class DeleteCardHandler(ICardRepository _cardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<DeleteCardCommand>
{
    public async Task Handle(DeleteCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.Id);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var hasDeletePermission = await _cardRepository.IsUserAuthorizedToEditCardAsync(userId, request.Id);
        if (!hasDeletePermission)
        {
            var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
            var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

            if (currentUserMember == null)
            {
                throw new UnauthorizedAccessException("You don't have access to this workspace");
            }

            if (currentUserMember.Role == WorkspaceRole.Member && card.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("Members can only delete cards they created themselves");
            }

            throw new UnauthorizedAccessException("You don't have permission to delete this card");
        }

        await _cardRepository.DeleteAsync(request.Id);

        var cardDeletedEvent = new CardDeletedEvent
        {
            CardId = card.Id,
            BoardId = card.List?.BoardId ?? Guid.Empty,
            ListId = card.ListId,
            WorkspaceId = card.List?.Board?.WorkspaceId ?? Guid.Empty,
            UserId = userId,
            CardTitle = card.Title,
            Description = $"Card deleted: {card.Title}",
            Metadata = null,
            ActivityType = ActivityType.CardDeleted
        };
        await _rabbitMqPublisher.PublishAsync(cardDeletedEvent);
    }
}
