using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Card.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using System.Text.Json;

namespace Application.Card.Handlers;

internal class UnassignUserFromCardHandler(ICardRepository _cardRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<UnassignUserFromCardCommand, bool>
{
    public async Task<bool> Handle(UnassignUserFromCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.CardId);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var userToUnassign = await _userRepository.GetByIdAsync(request.UserId);
        if (userToUnassign == null)
        {
            throw new ArgumentException("User to unassign not found");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == currentUserId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        if (currentUserMember.Role == WorkspaceRole.Member)
        {
            throw new UnauthorizedAccessException("Members cannot unassign users from cards. Only workspace owners and admins can unassign members from cards");
        }

        bool canUnassign = currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin;

        if (!canUnassign)
        {
            throw new UnauthorizedAccessException("You don't have permission to unassign users from this card");
        }

        if (!await _cardRepository.IsUserCardMemberAsync(request.CardId, request.UserId))
        {
            throw new InvalidOperationException("User is not assigned to this card");
        }

        var result = await _cardRepository.UnassignUserFromCardAsync(request.CardId, request.UserId);

        if (result)
        {
            var unassignedEvent = new CardMemberUnassignedEvent
            {
                UserId = currentUserId,
                WorkspaceId = card.List.Board.WorkspaceId,
                BoardId = card.List.BoardId,
                ListId = card.ListId,
                CardId = request.CardId,
                CardTitle = card.Title,
                UnassignedUserId = request.UserId,
                UnassignedUserName = userToUnassign.Name,
                UnassignedUserEmail = userToUnassign.Email,
                Description = $"User {userToUnassign.Name} was unassigned from card {card.Title}",
                Metadata = JsonSerializer.Serialize(new { 
                    UnassignedUserId = request.UserId,
                    UnassignedUserName = userToUnassign.Name,
                    UnassignedUserEmail = userToUnassign.Email,
                    CardTitle = card.Title
                })
            };

            await _rabbitMqPublisher.PublishAsync(unassignedEvent);
        }

        return result;
    }
}