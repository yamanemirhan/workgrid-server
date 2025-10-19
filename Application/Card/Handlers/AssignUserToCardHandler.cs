using MediatR;
using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Card.Commands;
using Domain.Enums;
using Infrastructure.Messaging.RabbitMQ;
using Domain.Events;
using System.Text.Json;

namespace Application.Card.Handlers;

internal class AssignUserToCardHandler(ICardRepository _cardRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor,
        IRabbitMqPublisher _rabbitMqPublisher) : IRequestHandler<AssignUserToCardCommand, CardMemberDto>
{
    public async Task<CardMemberDto> Handle(AssignUserToCardCommand request, CancellationToken cancellationToken)
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

        var userToAssign = await _userRepository.GetByIdAsync(request.UserId);
        if (userToAssign == null)
        {
            throw new ArgumentException("User to assign not found");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
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

        if (currentUserMember.Role == WorkspaceRole.Member)
        {
            throw new UnauthorizedAccessException("Members cannot assign users to cards. Only workspace owners and admins can assign members to cards");
        }

        bool canAssign = currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin;

        if (!canAssign)
        {
            throw new UnauthorizedAccessException("You don't have permission to assign users to this card");
        }

        if (await _cardRepository.IsUserCardMemberAsync(request.CardId, request.UserId))
        {
            throw new InvalidOperationException("User is already assigned to this card");
        }

        var cardMember = new CardMember
        {
            CardId = request.CardId,
            UserId = request.UserId,
            AssignedBy = currentUserId,
            AssignedAt = DateTime.UtcNow
        };

        var assignedCardMember = await _cardRepository.AssignUserToCardAsync(cardMember);

        var assignedEvent = new CardMemberAssignedEvent
        {
            UserId = currentUserId,
            WorkspaceId = card.List.Board.WorkspaceId,
            BoardId = card.List.BoardId,
            ListId = card.ListId,
            CardId = request.CardId,
            CardTitle = card.Title,
            AssignedUserId = request.UserId,
            AssignedUserName = userToAssign.Name,
            AssignedUserEmail = userToAssign.Email,
            Description = $"User {userToAssign.Name} was assigned to card {card.Title}",
            Metadata = JsonSerializer.Serialize(new { 
                AssignedUserId = request.UserId,
                AssignedUserName = userToAssign.Name,
                AssignedUserEmail = userToAssign.Email,
                CardTitle = card.Title
            })
        };

        await _rabbitMqPublisher.PublishAsync(assignedEvent);

        return new CardMemberDto
        {
            Id = assignedCardMember.Id,
            CardId = assignedCardMember.CardId,
            UserId = assignedCardMember.UserId,
            UserName = userToAssign.Name,
            UserEmail = userToAssign.Email,
            UserAvatar = userToAssign.Avatar,
            AssignedAt = assignedCardMember.AssignedAt,
            AssignedBy = assignedCardMember.AssignedBy,
            AssignedByName = assignedCardMember.AssignedByUser?.Name ?? "Unknown"
        };
    }
}