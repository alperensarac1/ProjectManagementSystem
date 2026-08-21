using FluentValidation;
using ProjectManagement.Application.DTOs.Tasks;

namespace ProjectManagement.Application.Validators.Tasks;

public sealed class UpdateTaskStatusRequestValidator
    : AbstractValidator<UpdateTaskStatusRequestDto>
{
    public UpdateTaskStatusRequestValidator()
    {
        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("Geçerli bir görev durumu seçilmelidir.");
    }
}