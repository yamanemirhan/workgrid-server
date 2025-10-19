using Application.Comment.Commands;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Messaging.RabbitMQ;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.DTOs;
using System.Text.RegularExpressions;
using Domain.Entities;

namespace Application.Comment.Handlers;

internal class CreateCommentCommandHandler(ICommentRepository _commentRepository,
        ICardRepository _cardRepository,
        IWorkspaceRepository _workspaceRepository,
        IUserRepository _userRepository,
        IFileStorageService _fileStorageService,
        IRabbitMqPublisher _eventPublisher,
        IHttpContextAccessor _httpContextAccessor) : IRequestHandler<CreateCommentCommand, CommentDto>
{
    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var card = await _cardRepository.GetByIdWithDetailsAsync(request.CardId);
            if (card == null)
            {
                throw new ArgumentException("Card not found");
            }

            var isMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, card.List.Board.WorkspaceId);
            if (!isMember)
            {
                throw new UnauthorizedAccessException("You don't have permission to comment on this card");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }

            var contentPreview = string.IsNullOrEmpty(request.Content) ? "" : 
                request.Content.Length > 50 ? request.Content[..50] + "..." : request.Content;           
            
            var commentId = Guid.NewGuid();
            
            var comment = new Domain.Entities.Comment
            {
                Id = commentId,
                CardId = request.CardId,
                UserId = userId,
                Content = request.Content,
                IsEdited = false,
                IsDeleted = false
            };

            var createdComment = await _commentRepository.CreateAsync(comment);

            if (request.Attachments?.Any() == true)
            {              
                foreach (var file in request.Attachments)
                {
                    if (ValidateFile(file))
                    {
                        await AddFileAttachment(createdComment.Id, file, userId);
                    }
                }
            }

            // Handle mentions
            if (request.MentionedUserIds?.Any() == true)
            {               
                await ProcessMentions(createdComment.Id, request.Content, request.MentionedUserIds);
            }

            return new CommentDto
            {
                Id = createdComment.Id,
                CardId = createdComment.CardId,
                UserId = createdComment.UserId,
                Content = createdComment.Content,
                IsEdited = createdComment.IsEdited,
                IsDeleted = createdComment.IsDeleted,
                CreatedAt = createdComment.CreatedAt,
                UpdatedAt = createdComment.UpdatedAt,
                User = new UserDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Avatar = user.Avatar,
                    CreatedAt = user.CreatedAt
                },
                Reactions = new List<CommentReactionDto>(),
                Attachments = new List<CommentAttachmentDto>(),
                Mentions = new List<CommentMentionDto>()
            };
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private bool ValidateFile(IFormFile file)
    {
        const long maxFileSize = 10 * 1024 * 1024; // 10MB
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt" };

        if (file.Length > maxFileSize)
        {
            return false;
        }

        var extension = _fileStorageService.GetFileExtension(file.FileName);
        if (!allowedExtensions.Contains(extension))
        {
            return false;
        }

        return true;
    }
    private async Task<CommentAttachment> AddFileAttachment(Guid commentId, IFormFile file, Guid userId)
    {
        await using var originalStream = file.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await originalStream.CopyToAsync(memoryStream);
        var fileBytes = memoryStream.ToArray();

        await using var uploadStream = new MemoryStream(fileBytes);
        var fileUrl = await _fileStorageService.UploadFileAsync(
            uploadStream,
            file.FileName,
            file.ContentType,
            "comment-attachments");

        string? thumbnailUrl = null;
        if (_fileStorageService.IsImageFile(file.ContentType))
        {
            await using var thumbStream = new MemoryStream(fileBytes);
            thumbnailUrl = await _fileStorageService.GenerateThumbnailAsync(thumbStream, file.FileName);
        }

        var attachment = new CommentAttachment
        {
            Id = Guid.NewGuid(),
            CommentId = commentId,
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileType = file.ContentType,
            FileSize = file.Length,
            ThumbnailUrl = thumbnailUrl,
            CreatedAt = DateTime.UtcNow
        };

        return await _commentRepository.AddAttachmentAsync(attachment);
    }

    private async Task ProcessMentions(Guid commentId, string content, List<Guid> mentionedUserIds)
    {
        // Find @mention patterns in content and match with provided user IDs
        var mentionPattern = @"@\w+";
        var matches = Regex.Matches(content, mentionPattern);

        for (int i = 0; i < matches.Count && i < mentionedUserIds.Count; i++)
        {
            var match = matches[i];
            var mention = new CommentMention
            {
                Id = Guid.NewGuid(),
                CommentId = commentId,
                MentionedUserId = mentionedUserIds[i],
                StartIndex = match.Index,
                Length = match.Length,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepository.AddMentionAsync(mention);

            // Publish mention event for notification
            //await PublishUserMentionedEvent(mention);
        }
    }
}