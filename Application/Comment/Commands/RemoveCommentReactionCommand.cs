using MediatR;

namespace Application.Comment.Commands;

public sealed record RemoveCommentReactionCommand : IRequest<bool>
{
    public Guid CommentId { get; set; }
}