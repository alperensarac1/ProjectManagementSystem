using FluentValidation;
using ProjectManagement.Application.DTOs.Users;

namespace ProjectManagement.Application.Validators.Users;
public sealed class UpdateUserRequestValidator
    : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(request => request.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kullanıcı adı zorunludur.")
            .MaximumLength(50)
            .WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.");

        RuleFor(request => request.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Kullanıcı soyadı zorunludur.")
            .MaximumLength(50)
            .WithMessage("Kullanıcı soyadı en fazla 50 karakter olabilir.");

        RuleFor(request => request.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("E-posta adresi zorunludur.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi girilmelidir.")
            .MaximumLength(200)
            .WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

        RuleFor(request => request.Role)
            .IsInEnum()
            .WithMessage("Geçerli bir kullanıcı rolü seçilmelidir.");

        RuleFor(request => request.Department)
            .MaximumLength(100)
            .WithMessage("Departman en fazla 100 karakter olabilir.")
            .When(request =>
                !string.IsNullOrWhiteSpace(request.Department));
    }
}