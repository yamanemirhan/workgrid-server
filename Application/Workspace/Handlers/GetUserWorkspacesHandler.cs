using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Workspace.Queries;

namespace Application.Workspace.Handlers;

internal class GetUserWorkspacesHandler(IWorkspaceRepository _workspaceRepository,
IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetUserWorkspacesQuery, IEnumerable<WorkspaceDto>>
{
    public async Task<IEnumerable<WorkspaceDto>> Handle(GetUserWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspaces = await _workspaceRepository.GetUserWorkspacesAsync(userId);

        return workspaces.Select(w => new WorkspaceDto
        {
            Id = w.Id,
            Name = w.Name,
            Description = w.Description,
            Logo = w.Logo,
            OwnerId = w.OwnerId,
            OwnerName = w.Owner?.Name ?? "Unknown",
            CreatedAt = w.CreatedAt,
            MemberCount = w.Members?.Count ?? 0,
            BoardCount = w.Boards?.Count ?? 0,
            Subscription = w.Subscription != null ? new SubscriptionDto
            {
                Id = w.Subscription.Id,
                WorkspaceId = w.Subscription.WorkspaceId,
                Plan = w.Subscription.Plan.ToString(),
                Status = w.Subscription.Status.ToString(),
                StripeCustomerId = w.Subscription.StripeCustomerId,
                StripeSubscriptionId = w.Subscription.StripeSubscriptionId,
                CurrentPeriodStart = w.Subscription.CurrentPeriodStart,
                CurrentPeriodEnd = w.Subscription.CurrentPeriodEnd,
                CreatedAt = w.Subscription.CreatedAt
            } : null
        }).OrderBy(w => w.Name);
    }
}