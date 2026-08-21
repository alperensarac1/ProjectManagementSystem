using FluentValidation;
using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Application.Validators.Auth;

public sealed class RefreshTokenRequestValidator
    : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token zorunludur.");
    }
}