using FluentValidation;
using ProjectManagement.Application.DTOs.Mailbox;

namespace ProjectManagement.Application.Validators.Mailbox;

public sealed class MailboxListQueryValidator
    : AbstractValidator<MailboxListQueryDto>
{
    public MailboxListQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage(
                "Sayfa numarası sıfırdan büyük olmalıdır.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        RuleFor(query => query.Search)
            .MaximumLength(250)
            .WithMessage(
                "Arama ifadesi en fazla 250 karakter olabilir.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));
    }
}