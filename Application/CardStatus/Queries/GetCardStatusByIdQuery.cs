using MediatR;
using Shared.DTOs;

namespace Application.CardStatus.Queries;

public sealed record GetCardStatusByIdQuery : IRequest<CardStatusDto?>
{
    public Guid Id { get; set; }
}