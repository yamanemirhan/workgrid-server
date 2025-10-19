using MediatR;

namespace Application.List.Commands;

public sealed record DeleteListCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}