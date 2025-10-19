using MediatR;
using Shared.DTOs;

namespace Application.List.Commands;

public sealed record CreateListCommand : IRequest<ListDto>
{
    public string Title { get; set; } = string.Empty;
    public Guid BoardId { get; set; }
}