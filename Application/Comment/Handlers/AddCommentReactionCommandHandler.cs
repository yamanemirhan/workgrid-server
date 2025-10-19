using Application.Comment.Commands;
using Domain.Entities;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Comment.Handlers;

internal class AddCommentReactionCommandHandler(ICommentRepository _commentRepository,
        IWorkspaceRepository _workspaceRepository,
        ICardRepository _cardRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<AddCommentReactionCommand, CommentReactionDto>
{
    public async Task<CommentReactionDto> Handle(AddCommentReactionCommand request, CancellationToken cancellationToken)
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
                throw new UnauthorizedAccessException("You don't have permission to react to this comment");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var reaction = new CommentReaction
            {
                Id = Guid.NewGuid(),
                CommentId = request.CommentId,
                UserId = userId,
                Emoji = request.Emoji,
                CreatedAt = DateTime.UtcNow
            };

            var addedReaction = await _commentRepository.AddReactionAsync(reaction);

            // Publish domain event if needed
            // await PublishCommentReactionAddedEvent(addedReaction);

            return new CommentReactionDto
            {
                Id = addedReaction.Id,
                CommentId = addedReaction.CommentId,
                UserId = addedReaction.UserId,
                Emoji = addedReaction.Emoji,
                CreatedAt = addedReaction.CreatedAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Avatar = user.Avatar,
                    CreatedAt = user.CreatedAt
                }
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}