using MediatR;
using Application.Auth.Commands;

namespace Application.Auth.Handlers;

internal class LogoutUserHandler : IRequestHandler<LogoutUserCommand, bool>
{
    public async Task<bool> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {           
        return await Task.FromResult(true);
    }
}