using MediatR;
using Shared.DTOs;

namespace Application.CardStatus.Queries;

public sealed record GetWorkspaceCardStatusesQuery : IRequest<IEnumerable<CardStatusDto>>
{
    public Guid WorkspaceId { get; set; }
}