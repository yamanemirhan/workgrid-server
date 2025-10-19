using MediatR;
using Application.Auth.Commands;
using Application.Auth.Services;
using Shared.DTOs;

namespace Application.Auth.Handlers;

internal class LoginUserHandler(IAuthService _authService) : IRequestHandler<LoginUserCommand, LoginResponseDto>
{
    public async Task<LoginResponseDto> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password);
    }
}