using MediatR;

namespace Application.Card.Commands;

public sealed record UnassignUserFromCardCommand : IRequest<bool>
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
}