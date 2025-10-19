using Shared.DTOs;
using MediatR;

namespace Application.Comment.Commands;

public sealed record UpdateCommentCommand : IRequest<CommentDto>
{
    public Guid CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<Guid>? MentionedUserIds { get; set; }
}