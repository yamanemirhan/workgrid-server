using MediatR;

namespace Application.Card.Commands;

public sealed record DeleteCardCommand : IRequest
{
    public Guid Id { get; set; }
}
