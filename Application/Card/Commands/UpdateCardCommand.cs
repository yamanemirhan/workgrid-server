using MediatR;
using Shared.DTOs;

namespace Application.Card.Commands;

public sealed record UpdateCardCommand : IRequest<CardDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? EstimatedTime { get; set; }
    public string? Tags { get; set; }
}
