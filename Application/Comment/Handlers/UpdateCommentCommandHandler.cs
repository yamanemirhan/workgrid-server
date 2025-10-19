using Application.Comment.Commands;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Comment.Handlers;

internal class UpdateCommentCommandHandler(ICommentRepository _commentRepository,
        IUserRepository _userRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<UpdateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
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

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only update your own comments");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        comment.Content = request.Content;
        comment.IsEdited = true;
        comment.UpdatedAt = DateTime.UtcNow;

        var updatedComment = await _commentRepository.UpdateAsync(comment);

        // TODO: Handle mentions

        return new CommentDto
        {
            Id = updatedComment.Id,
            CardId = updatedComment.CardId,
            UserId = updatedComment.UserId,
            Content = updatedComment.Content,
            IsEdited = updatedComment.IsEdited,
            IsDeleted = updatedComment.IsDeleted,
            CreatedAt = updatedComment.CreatedAt,
            UpdatedAt = updatedComment.UpdatedAt,
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
}