using FluentValidation;
using ProjectManagement.Application.DTOs.Projects;

namespace ProjectManagement.Application.Validators.Projects;

public sealed class ProjectListQueryValidator
    : AbstractValidator<ProjectListQueryDto>
{
    public ProjectListQueryValidator()
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

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir proje durumu seçilmelidir.")
            .When(query => query.Status.HasValue);

        RuleFor(query => query.OwnerId)
            .GreaterThan(0)
            .WithMessage("Proje sahibi ID değeri geçerli değildir.")
            .When(query => query.OwnerId.HasValue);
    }
}