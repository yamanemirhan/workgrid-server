using MediatR;
using Microsoft.AspNetCore.Http;
using Infrastructure.Repositories;
using Shared.DTOs;
using Application.Member.Queries;

namespace Application.Member.Handlers;

internal class GetWorkspaceMembersHandler(IWorkspaceRepository _workspaceRepository,
    IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetWorkspaceMembersQuery, IEnumerable<MemberDto>>
{
    public async Task<IEnumerable<MemberDto>> Handle(GetWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var workspace = await _workspaceRepository.GetByIdAsync(request.WorkspaceId);
        if (workspace == null)
        {
            throw new ArgumentException("Workspace not found");
        }

        var isUserMember = await _workspaceRepository.IsUserMemberOfWorkspaceAsync(currentUserId, request.WorkspaceId);
        if (!isUserMember)
        {
            throw new UnauthorizedAccessException("You don't have permission to view members of this workspace. Only workspace members can view the member list.");
        }

        var members = await _workspaceRepository.GetWorkspaceMembersAsync(request.WorkspaceId);

        return members.Select(member => new MemberDto
        {
            Id = member.Id,
            UserId = member.UserId,
            WorkspaceId = member.WorkspaceId,
            UserName = member.User?.Name ?? "Unknown User",
            UserEmail = member.User?.Email ?? "unknown@email.com",
            UserAvatar = member.User?.Avatar,
            Role = member.Role,
            JoinedAt = member.JoinedAt
        });
    }
}