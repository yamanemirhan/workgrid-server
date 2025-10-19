using MediatR;
using Shared.DTOs;

namespace Application.List.Queries;

public sealed record GetBoardListsQuery : IRequest<IEnumerable<ListDto>>
{
    public Guid BoardId { get; set; }
}