using MediatR;

namespace Application.Auth.Commands;

public sealed record LogoutUserCommand : IRequest<bool>
{
}