using MediatR;
using Shared.DTOs;

namespace Application.List.Commands;

public sealed record UpdateListCommand : IRequest<ListDto>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
}