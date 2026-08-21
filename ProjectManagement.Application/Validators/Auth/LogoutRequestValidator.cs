using FluentValidation;
using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Application.Validators.Auth;

public sealed class LogoutRequestValidator
    : AbstractValidator<LogoutRequestDto>
{
    public LogoutRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token zorunludur.");
    }
}