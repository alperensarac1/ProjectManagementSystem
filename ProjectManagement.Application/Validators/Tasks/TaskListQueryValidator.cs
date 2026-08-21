using FluentValidation;
using ProjectManagement.Application.DTOs.Tasks;

namespace ProjectManagement.Application.Validators.Tasks;

public sealed class TaskListQueryValidator
    : AbstractValidator<TaskListQueryDto>
{
    public TaskListQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("Sayfa numarası sıfırdan büyük olmalıdır.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        RuleFor(query => query.Search)
            .MaximumLength(200)
            .WithMessage("Arama ifadesi en fazla 200 karakter olabilir.")
            .When(query =>
                !string.IsNullOrWhiteSpace(query.Search));

        RuleFor(query => query.ProjectId)
            .GreaterThan(0)
            .WithMessage("Proje ID değeri geçerli değildir.")
            .When(query => query.ProjectId.HasValue);

        RuleFor(query => query.AssignedToUserId)
            .GreaterThan(0)
            .WithMessage("Kullanıcı ID değeri geçerli değildir.")
            .When(query => query.AssignedToUserId.HasValue);

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir görev durumu seçilmelidir.")
            .When(query => query.Status.HasValue);

        RuleFor(query => query.Priority)
            .IsInEnum()
            .WithMessage("Geçerli bir görev önceliği seçilmelidir.")
            .When(query => query.Priority.HasValue);
    }
}