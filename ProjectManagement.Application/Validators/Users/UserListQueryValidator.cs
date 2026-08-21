using FluentValidation;
using ProjectManagement.Application.DTOs.Users;

namespace ProjectManagement.Application.Validators.Users;

public sealed class UserListQueryValidator
    : AbstractValidator<UserListQueryDto>
{
    public UserListQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası sıfırdan büyük olmalıdır.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        RuleFor(query => query.Search)
            .MaximumLength(200)
            .WithMessage("Arama ifadesi en fazla 200 karakter olabilir.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.Role)
            .IsInEnum()
            .WithMessage("Geçerli bir kullanıcı rolü seçilmelidir.")
            .When(query => query.Role.HasValue);
    }
}