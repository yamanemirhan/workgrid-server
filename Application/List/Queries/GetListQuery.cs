using MediatR;
using Shared.DTOs;

namespace Application.List.Queries;

public sealed record GetListQuery : IRequest<ListDto?>
{
    public Guid Id { get; set; }
}