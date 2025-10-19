using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Workspace.Queries;

namespace Application.Workspace.Handlers;

internal class GetWorkspaceHandler(IWorkspaceRepository _workspaceRepository,
   IUserRepository _userRepository,
   IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetWorkspaceQuery, WorkspaceDto?>
{
    public async Task<WorkspaceDto?> Handle(GetWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspace = await _workspaceRepository.GetByIdWithDetailsAsync(request.Id);
        if (workspace == null)
        {
            return null;
        }

        var hasAccess = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(userId, request.Id);
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("You don't have access to this workspace");
        }

        var owner = await _userRepository.GetByIdAsync(workspace.OwnerId);

        return new WorkspaceDto
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Logo = workspace.Logo,
            OwnerId = workspace.OwnerId,
            OwnerName = owner?.Name ?? "Unknown",
            CreatedAt = workspace.CreatedAt,
            MemberCount = workspace.Members?.Count() ?? 1,
            BoardCount = workspace.Boards?.Count() ?? 0,
            Subscription = workspace.Subscription != null ? new SubscriptionDto
            {
                Id = workspace.Subscription.Id,
                WorkspaceId = workspace.Subscription.WorkspaceId,
                Plan = workspace.Subscription.Plan.ToString(),
                Status = workspace.Subscription.Status.ToString(),
                StripeCustomerId = workspace.Subscription.StripeCustomerId,
                StripeSubscriptionId = workspace.Subscription.StripeSubscriptionId,
                CurrentPeriodStart = workspace.Subscription.CurrentPeriodStart,
                CurrentPeriodEnd = workspace.Subscription.CurrentPeriodEnd,
                CreatedAt = workspace.Subscription.CreatedAt
            } : null
        };
    }
}