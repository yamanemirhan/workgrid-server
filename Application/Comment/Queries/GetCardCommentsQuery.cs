using Shared.DTOs;
using MediatR;

namespace Application.Comment.Queries;

public sealed record GetCardCommentsQuery : IRequest<IEnumerable<CommentDto>>
{
    public Guid CardId { get; set; }
}