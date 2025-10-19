using Application.Comment.Commands;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Comment.Handlers;

internal class RemoveCommentReactionCommandHandler(ICommentRepository _commentRepository,
        IWorkspaceRepository _workspaceRepository,
        ICardRepository _cardRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<RemoveCommentReactionCommand, bool>
{
    public async Task<bool> Handle(RemoveCommentReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var comment = await _commentRepository.GetByIdAsync(request.CommentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found");
            }

            var card = await _cardRepository.GetByIdWithDetailsAsync(comment.CardId);
            if (card == null)
            {
                throw new ArgumentException("Card not found");
            }

            var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, card.List.Board.WorkspaceId);
            if (!isMember)
            {
                throw new UnauthorizedAccessException("You don't have permission to remove reaction from this comment");
            }

            var result = await _commentRepository.RemoveReactionAsync(request.CommentId, userId);

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}