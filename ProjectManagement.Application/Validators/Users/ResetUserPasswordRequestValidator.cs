using FluentValidation;
using ProjectManagement.Application.DTOs.Users;

namespace ProjectManagement.Application.Validators.Users;


public sealed class ResetUserPasswordRequestValidator
    : AbstractValidator<ResetUserPasswordRequestDto>
{
    public ResetUserPasswordRequestValidator()
    {
        RuleFor(request => request.NewPassword)
            .Cascade(CascadeMode.Stop)

            .NotEmpty()
            .WithMessage("Yeni şifre zorunludur.")

            .MinimumLength(8)
            .WithMessage(
                "Yeni şifre en az 8 karakter olmalıdır.")

            .MaximumLength(100)
            .WithMessage(
                "Yeni şifre en fazla 100 karakter olabilir.")

            .Matches("[A-Z]")
            .WithMessage(
                "Yeni şifre en az bir büyük harf içermelidir.")

            .Matches("[a-z]")
            .WithMessage(
                "Yeni şifre en az bir küçük harf içermelidir.")

            .Matches("[0-9]")
            .WithMessage(
                "Yeni şifre en az bir rakam içermelidir.");
    }
}