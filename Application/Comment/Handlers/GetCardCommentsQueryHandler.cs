using Application.Comment.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Comment.Handlers;

internal class GetCardCommentsQueryHandler(ICommentRepository _commentRepository,
        ICardRepository _cardRepository,
        IWorkspaceRepository _workspaceRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetCardCommentsQuery, IEnumerable<CommentDto>>
{
    public async Task<IEnumerable<CommentDto>> Handle(GetCardCommentsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User ID not found in token");

        // Validate user has access to the card
        if (!await ValidateUserAccessToCard(userId, request.CardId))
            throw new UnauthorizedAccessException("User does not have access to this card");

        var comments = await _commentRepository.GetByCardIdAsync(request.CardId, includeReplies: false);

        // Map comments to DTOs
        return comments.Select(comment => MapCommentToDto(comment));
    }

    private CommentDto MapCommentToDto(Domain.Entities.Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            CardId = comment.CardId,
            UserId = comment.UserId,
            Content = comment.Content,
            IsEdited = comment.IsEdited,
            IsDeleted = comment.IsDeleted,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt,
            User = comment.User != null ? new UserDto
            {
                Id = comment.User.Id,
                Name = comment.User.Name,
                Email = comment.User.Email,
                Avatar = comment.User.Avatar,
                CreatedAt = comment.User.CreatedAt
            } : null!,
            Reactions = comment.Reactions?.Select(r => new CommentReactionDto
            {
                Id = r.Id,
                CommentId = r.CommentId,
                UserId = r.UserId,
                Emoji = r.Emoji,
                CreatedAt = r.CreatedAt,
                User = r.User != null ? new UserDto
                {
                    Id = r.User.Id,
                    Name = r.User.Name,
                    Email = r.User.Email,
                    Avatar = r.User.Avatar,
                    CreatedAt = r.User.CreatedAt
                } : null!
            }).ToList(),
            Attachments = comment.Attachments?.Select(a => new CommentAttachmentDto
            {
                Id = a.Id,
                CommentId = a.CommentId,
                FileName = a.FileName,
                FilePath = a.FileUrl,
                ContentType = a.FileType,
                FileSize = a.FileSize,
                ThumbnailUrl = a.ThumbnailUrl,
                CreatedAt = a.CreatedAt
            }).ToList(),
            Mentions = comment.Mentions?.Select(m => new CommentMentionDto
            {
                Id = m.Id,
                CommentId = m.CommentId,
                MentionedUserId = m.MentionedUserId,
                StartIndex = m.StartIndex,
                Length = m.Length,
                CreatedAt = m.CreatedAt,
                MentionedUser = m.MentionedUser != null ? new UserDto
                {
                    Id = m.MentionedUser.Id,
                    Name = m.MentionedUser.Name,
                    Email = m.MentionedUser.Email,
                    Avatar = m.MentionedUser.Avatar,
                    CreatedAt = m.MentionedUser.CreatedAt
                } : null!
            }).ToList()
        };
    }

    private async Task<bool> ValidateUserAccessToCard(Guid userId, Guid cardId)
    {
        var card = await _cardRepository.GetByIdAsync(cardId);
        if (card == null) return false;

        if (card.List == null || card.List.Board == null) return false;

        return await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, card.List.Board.WorkspaceId);
    }
}