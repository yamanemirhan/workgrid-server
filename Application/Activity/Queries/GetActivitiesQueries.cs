using MediatR;
using Shared.DTOs;

namespace Application.Activity.Queries;

public sealed record GetWorkspaceActivitiesQuery : IRequest<IEnumerable<ActivityDto>>
{
    public Guid WorkspaceId { get; set; }
    public int Take { get; set; } = 50;
}

public sealed record GetBoardActivitiesQuery : IRequest<IEnumerable<ActivityDto>>
{
    public Guid BoardId { get; set; }
    public int Take { get; set; } = 50;
}

public sealed record GetListActivitiesQuery : IRequest<IEnumerable<ActivityDto>>
{
    public Guid ListId { get; set; }
    public int Take { get; set; } = 50;
}

public sealed record GetCardActivitiesQuery : IRequest<IEnumerable<ActivityDto>>
{
    public Guid CardId { get; set; }
    public int Take { get; set; } = 50;
}