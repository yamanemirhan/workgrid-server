using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Application.Card.Commands;
using Domain.Enums;

namespace Application.Card.Handlers;

internal class UnfollowCardHandler(ICardRepository _cardRepository,
        IBoardRepository _boardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UnfollowCardCommand, bool>
{
    public async Task<bool> Handle(UnfollowCardCommand request, CancellationToken cancellationToken)
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
            if (!card.List.Board.IsPrivate || await _boardRepository.IsUserBoardMemberAsync(card.List.BoardId, currentUserId))
            {
                canAccess = true;
            }
        }

        if (!canAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to access this card");
        }

        if (!await _cardRepository.IsUserFollowingCardAsync(request.CardId, currentUserId))
        {
            throw new InvalidOperationException("You are not following this card");
        }

        return await _cardRepository.UnfollowCardAsync(request.CardId, currentUserId);
    }
}