using FluentValidation;
using ProjectManagement.Application.DTOs.Auth;

namespace ProjectManagement.Application.Validators.Auth;


public sealed class ChangePasswordRequestValidator
    : AbstractValidator<ChangePasswordRequestDto>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Mevcut şifre zorunludur.");

        RuleFor(request => request.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Yeni şifre zorunludur.")
            .MinimumLength(8)
            .WithMessage("Yeni şifre en az 8 karakter olmalıdır.")
            .MaximumLength(100)
            .WithMessage("Yeni şifre en fazla 100 karakter olabilir.")
            .Must(password => password.Any(char.IsUpper))
            .WithMessage("Yeni şifre en az bir büyük harf içermelidir.")
            .Must(password => password.Any(char.IsLower))
            .WithMessage("Yeni şifre en az bir küçük harf içermelidir.")
            .Must(password => password.Any(char.IsDigit))
            .WithMessage("Yeni şifre en az bir rakam içermelidir.");

        RuleFor(request => request.ConfirmNewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Yeni şifre tekrarı zorunludur.")
            .Equal(request => request.NewPassword)
            .WithMessage("Yeni şifre ve şifre tekrarı eşleşmelidir.");

        RuleFor(request => request.NewPassword)
            .NotEqual(request => request.CurrentPassword)
            .WithMessage(
                "Yeni şifre mevcut şifreyle aynı olamaz.")
            .When(request =>
                !string.IsNullOrWhiteSpace(request.CurrentPassword) &&
                !string.IsNullOrWhiteSpace(request.NewPassword));
    }
}