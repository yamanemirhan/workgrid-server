using MediatR;

namespace Application.CardStatus.Commands;

public sealed record DeleteCardStatusCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}