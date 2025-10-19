using MediatR;
using Microsoft.AspNetCore.Http;
using Domain.Entities;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Card.Commands;
using Domain.Enums;

namespace Application.Card.Handlers;

internal class FollowCardHandler(ICardRepository _cardRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<FollowCardCommand, CardFollowerDto>
{
    public async Task<CardFollowerDto> Handle(FollowCardCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var user = await _userRepository.GetByIdAsync(currentUserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.CardId);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(card.List.Board.WorkspaceId);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == currentUserId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        bool canAccess = false;
        if (currentUserMember.Role == WorkspaceRole.Owner || currentUserMember.Role == WorkspaceRole.Admin)
        {
            canAccess = true;
        }
        else if (currentUserMember.Role == WorkspaceRole.Member)
        {
            // Member can access if board is public or they are assigned to the board
            if (!card.List.Board.IsPrivate || await _boardRepository.IsUserBoardMemberAsync(card.List.BoardId, currentUserId))
            {
                canAccess = true;
            }
        }

        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to access this card");
        }

        // Check if user is already following card
        if (await _cardRepository.IsUserFollowingCardAsync(request.CardId, currentUserId))
        {
            throw new InvalidOperationException("You are already following this card");
        }

        var cardFollower = new CardFollower
        {
            CardId = request.CardId,
            UserId = currentUserId,
            FollowedAt = DateTime.UtcNow
        };

        var followedCard = await _cardRepository.FollowCardAsync(cardFollower);

        return new CardFollowerDto
        {
            Id = followedCard.Id,
            CardId = followedCard.CardId,
            UserId = followedCard.UserId,
            UserName = user.Name,
            UserEmail = user.Email,
            UserAvatar = user.Avatar,
            FollowedAt = followedCard.FollowedAt
        };
    }
}