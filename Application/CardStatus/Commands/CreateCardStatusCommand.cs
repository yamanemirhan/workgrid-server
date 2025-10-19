using MediatR;
using Shared.DTOs;

namespace Application.CardStatus.Commands;

public sealed record CreateCardStatusCommand : IRequest<CardStatusDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#6B7280";
    public Guid WorkspaceId { get; set; }
}