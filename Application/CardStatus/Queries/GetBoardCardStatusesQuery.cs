using MediatR;
using Shared.DTOs;

namespace Application.CardStatus.Queries;

public sealed record GetBoardCardStatusesQuery : IRequest<IEnumerable<CardStatusDto>>
{
    public Guid BoardId { get; set; }
    public Guid WorkspaceId { get; set; }
}