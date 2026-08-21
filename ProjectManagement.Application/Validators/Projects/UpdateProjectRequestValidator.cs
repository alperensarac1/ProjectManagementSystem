using FluentValidation;
using ProjectManagement.Application.DTOs.Projects;

namespace ProjectManagement.Application.Validators.Projects;

public sealed class UpdateProjectRequestValidator
    : AbstractValidator<UpdateProjectRequestDto>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Proje adı zorunludur.")
            .MaximumLength(200)
            .WithMessage("Proje adı en fazla 200 karakter olabilir.");

        RuleFor(request => request.Description)
            .MaximumLength(5000)
            .WithMessage(
                "Proje açıklaması en fazla 5000 karakter olabilir.")
            .When(request =>
                !string.IsNullOrWhiteSpace(request.Description));

        RuleFor(request => request.StartDate)
            .NotEmpty()
            .WithMessage("Proje başlangıç tarihi zorunludur.");

        RuleFor(request => request.EndDate)
            .GreaterThanOrEqualTo(request => request.StartDate)
            .WithMessage(
                "Proje bitiş tarihi başlangıç tarihinden önce olamaz.")
            .When(request => request.EndDate.HasValue);

        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir proje durumu seçilmelidir.");

        RuleFor(request => request.OwnerId)
            .GreaterThan(0)
            .WithMessage("Proje sahibi ID değeri geçerli değildir.")
            .When(request => request.OwnerId.HasValue);
    }
}