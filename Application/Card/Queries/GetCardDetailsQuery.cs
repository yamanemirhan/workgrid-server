using MediatR;
using Shared.DTOs;

namespace Application.Card.Queries;

public sealed record GetCardDetailsQuery : IRequest<CardDto>
{
    public Guid Id { get; set; }
}
