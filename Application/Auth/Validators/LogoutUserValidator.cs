using FluentValidation;
using Application.Auth.Commands;

namespace Application.Auth.Validators;

public class LogoutUserValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserValidator()
    {
    }
}