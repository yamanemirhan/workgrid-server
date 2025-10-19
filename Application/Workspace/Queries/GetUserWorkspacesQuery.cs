using MediatR;
using Shared.DTOs;

namespace Application.Workspace.Queries;

public sealed record GetUserWorkspacesQuery : IRequest<IEnumerable<WorkspaceDto>>
{
}