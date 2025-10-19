using MediatR;
using Shared.DTOs;

namespace Application.Card.Commands;

public sealed record CreateCardCommand : IRequest<CardDto>
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ListId { get; set; }
    public string? EstimatedTime { get; set; }
    public string? Tags { get; set; }
}
