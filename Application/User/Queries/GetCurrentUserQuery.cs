using MediatR;
using Shared.DTOs;

namespace Application.User.Queries;

public sealed record GetCurrentUserQuery : IRequest<UserDetailDto>
{
}