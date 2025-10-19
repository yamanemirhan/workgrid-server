using Application.Comment.Commands;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Comment.Handlers;

internal class DeleteCommentCommandHandler(ICommentRepository _commentRepository,
        IWorkspaceRepository _workspaceRepository,
        ICardRepository _cardRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<DeleteCommentCommand, bool>
{
    public async Task<bool> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("You don't have permission to delete this comment");
            }

            if (comment.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only delete your own comments");
            }

            var result = await _commentRepository.SoftDeleteAsync(request.CommentId);

            // Publish domain event if needed
            // await PublishCommentDeletedEvent(comment);

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}