using Application.Card.Queries;
using Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;

namespace Application.Card.Handlers;

internal class GetCardDetailsHandler(ICardRepository _cardRepository,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetCardDetailsQuery, CardDto>
{
    public async Task<CardDto> Handle(GetCardDetailsQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var card = await _cardRepository.GetByIdWithDetailsAsync(request.Id);
        if (card == null)
        {
            throw new ArgumentException("Card not found");
        }

        var hasAccess = await _cardRepository.IsUserAuthorizedToAccessCardAsync(userId, request.Id);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You don't have permission to view this card");
        }

        return new CardDto
        {
            Id = card.Id,
            Title = card.Title,
            Description = card.Description,
            Position = card.Position,
            ListId = card.ListId,
            ListTitle = card.List?.Title ?? "Unknown",
            CreatedBy = card.CreatedBy,
            CreatorName = card.Creator?.Name ?? "Unknown",
            Creator = card.Creator != null ? new UserDto
            {
                Id = card.Creator.Id,
                Name = card.Creator.Name,
                Email = card.Creator.Email,
                Avatar = card.Creator.Avatar,
                CreatedAt = card.Creator.CreatedAt
            } : null,
            EstimatedTime = card.EstimatedTime,
            Tags = card.Tags,
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            StatusId = card.StatusId,
            Status = card.Status != null ? new CardStatusDto
            {
                Id = card.Status.Id,
                Name = card.Status.Name,
                Color = card.Status.Color,
                Position = card.Status.Position,
                IsDefault = card.Status.IsDefault,
                Description = card.Status.Description,
                Type = card.Status.Type,
                UpdatedAt = card.Status.UpdatedAt,
                CreatedAt = card.Status.CreatedAt,
                WorkspaceId = card.Status.WorkspaceId,
                WorkspaceName = card.Status.Workspace?.Name,
            } : null,
            CardMembers = card.CardMembers?.Select(cm => new CardMemberDto
            {
                Id = cm.Id,
                CardId = cm.CardId,
                UserId = cm.UserId,
                UserName = cm.User?.Name ?? "Unknown",
                UserEmail = cm.User?.Email ?? "Unknown",
                UserAvatar = cm.User?.Avatar,
                AssignedAt = cm.AssignedAt,
                AssignedBy = cm.AssignedBy,
                AssignedByName = cm.AssignedByUser?.Name ?? "Unknown"
            }),
            CardFollowers = card.CardFollowers?.Select(cf => new CardFollowerDto
            {
                Id = cf.Id,
                CardId = cf.CardId,
                UserId = cf.UserId,
                UserName = cf.User?.Name ?? "Unknown",
                UserEmail = cf.User?.Email ?? "Unknown",
                UserAvatar = cf.User?.Avatar,
                FollowedAt = cf.FollowedAt
            }),
            CardComments = card.Comments?
                .Where(c => !c.IsDeleted)
                .Select(c => MapCommentToDto(c))
        };
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
            }),
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
            }),
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
            })
        };
    }
}