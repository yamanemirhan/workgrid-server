using MediatR;
using Microsoft.AspNetCore.Http;
using Application.User.Queries;
using Shared.DTOs;
using Application.User.Services;

namespace Application.User.Handlers;

internal class GetCurrentUserHandler(IUserService _userService,
IHttpContextAccessor _httpContextAccessor) : IRequestHandler<GetCurrentUserQuery, UserDetailDto>
{
    public async Task<UserDetailDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        return await _userService.GetCurrentUserWithDetailsAsync(userId);
    }
}