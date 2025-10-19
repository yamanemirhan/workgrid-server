using MediatR;
using Application.Auth.Commands;
using Application.Auth.Services;
using Shared.DTOs;

namespace Application.Auth.Handlers;

internal class RegisterUserHandler(IAuthService _authService) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RegisterAsync(request.Name, request.Email, request.Password);
    }
}