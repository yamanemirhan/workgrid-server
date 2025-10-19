using Application.CardStatus.Commands;
using Domain.Enums;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.CardStatus.Handlers;

internal class DeleteCardStatusHandler(ICardStatusRepository _cardStatusRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteCardStatusCommand, bool>
{
    public async Task<bool> Handle(DeleteCardStatusCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var cardStatus = await _cardStatusRepository.GetByIdAsync(request.Id);
        if (cardStatus == null)
        {
            throw new ArgumentException("Card status not found");
        }

        // Check if it's a default status
        if (cardStatus.IsDefault)
        {
            throw new InvalidOperationException("Default statuses cannot be deleted");
        }

        if (!cardStatus.WorkspaceId.HasValue)
        {
            throw new InvalidOperationException("Cannot delete status without workspace");
        }

        var workspaceMembers = await _workspaceRepository.GetWorkspaceMembersAsync(cardStatus.WorkspaceId.Value);
        var currentUserMember = workspaceMembers.FirstOrDefault(m => m.UserId == userId);

        if (currentUserMember == null)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        // Only workspace owners and admins can delete custom statuses
        if (currentUserMember.Role != WorkspaceRole.Owner && currentUserMember.Role != WorkspaceRole.Admin)
        {
            throw new UnauthorizedAccessException("Only workspace owners and admins can delete custom card statuses");
        }

        // Check if status can be deleted (no cards using it)
        var canDelete = await _cardStatusRepository.CanDeleteStatusAsync(request.Id);
        if (!canDelete)
        {
            throw new InvalidOperationException("Cannot delete status that is currently being used by cards. Please change the status of all cards using this status first.");
        }

        return await _cardStatusRepository.DeleteAsync(request.Id);
    }
}