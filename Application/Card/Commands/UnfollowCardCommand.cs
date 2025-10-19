using MediatR;

namespace Application.Card.Commands;

public sealed record UnfollowCardCommand : IRequest<bool>
{
    public Guid CardId { get; set; }
}