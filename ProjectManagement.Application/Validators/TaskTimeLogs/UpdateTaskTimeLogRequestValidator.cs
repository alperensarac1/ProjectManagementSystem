using FluentValidation;
using ProjectManagement.Application.DTOs.TaskTimeLogs;

namespace ProjectManagement.Application.Validators.TaskTimeLogs;

public sealed class UpdateTaskTimeLogRequestValidator
    : AbstractValidator<UpdateTaskTimeLogRequestDto>
{
    public UpdateTaskTimeLogRequestValidator()
    {
        RuleFor(request => request.Hours)
            .GreaterThan(0)
            .WithMessage("Çalışma süresi sıfırdan büyük olmalıdır.")
            .LessThanOrEqualTo(24)
            .WithMessage(
                "Tek bir zaman kaydı için çalışma süresi 24 saati aşamaz.");

        RuleFor(request => request.Description)
            .MaximumLength(500)
            .WithMessage("Çalışma açıklaması en fazla 500 karakter olabilir.")
            .When(request =>
                !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.WorkDate)
            .NotEmpty()
            .WithMessage("Çalışma tarihi zorunludur.")
            .Must(workDate => workDate <= DateTime.UtcNow)
            .WithMessage("Çalışma tarihi gelecekte olamaz.");
    }
}