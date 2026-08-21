using FluentValidation;
using ProjectManagement.Application.DTOs.Tasks;

namespace ProjectManagement.Application.Validators.Tasks;

public sealed class CreateTaskRequestValidator
    : AbstractValidator<CreateTaskRequestDto>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(request => request.ProjectId)
            .GreaterThan(0)
            .WithMessage("Proje ID değeri geçerli değildir.");

        RuleFor(request => request.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Görev başlığı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Görev başlığı en fazla 200 karakter olabilir.");

        RuleFor(request => request.Description)
            .MaximumLength(5000)
            .WithMessage("Görev açıklaması en fazla 5000 karakter olabilir.")
            .When(request =>
                !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.AssignedToUserId)
            .GreaterThan(0)
            .WithMessage("Atanan kullanıcı ID değeri geçerli değildir.")
            .When(request => request.AssignedToUserId.HasValue);

        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir görev durumu seçilmelidir.");

        RuleFor(request => request.Priority)
            .IsInEnum()
            .WithMessage("Geçerli bir görev önceliği seçilmelidir.");

        RuleFor(request => request.EstimatedHours)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tahmini çalışma süresi negatif olamaz.")
            .When(request => request.EstimatedHours.HasValue);
    }
}