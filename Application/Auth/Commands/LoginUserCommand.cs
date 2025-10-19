using MediatR;
using Shared.DTOs;

namespace Application.Auth.Commands;

public sealed record LoginUserCommand : IRequest<LoginResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
