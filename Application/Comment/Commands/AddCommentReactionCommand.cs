using Shared.DTOs;
using MediatR;

namespace Application.Comment.Commands;

public sealed record AddCommentReactionCommand : IRequest<CommentReactionDto>
{
    public Guid CommentId { get; set; }
    public string Emoji { get; set; } = string.Empty;
}