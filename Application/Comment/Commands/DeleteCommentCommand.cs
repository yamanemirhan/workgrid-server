using MediatR;

namespace Application.Comment.Commands;

public sealed record DeleteCommentCommand : IRequest<bool>
{
    public Guid CommentId { get; set; }
}