using Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Comment.Commands;

public sealed record CreateCommentCommand : IRequest<CommentDto>
{
    public Guid CardId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<IFormFile>? Attachments { get; set; }
    public List<Guid>? MentionedUserIds { get; set; }
}