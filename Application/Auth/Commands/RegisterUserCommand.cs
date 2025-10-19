using MediatR;
using Shared.DTOs;

namespace Application.Auth.Commands;

public sealed record RegisterUserCommand : IRequest<UserDto>
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
