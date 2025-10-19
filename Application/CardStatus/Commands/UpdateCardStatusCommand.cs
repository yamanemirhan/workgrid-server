using MediatR;
using Shared.DTOs;

namespace Application.CardStatus.Commands;

public sealed record UpdateCardStatusCommand : IRequest<CardStatusDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
}