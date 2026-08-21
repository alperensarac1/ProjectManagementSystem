using FluentValidation;
using ProjectManagement.Application.DTOs.Tasks;

namespace ProjectManagement.Application.Validators.Tasks;

public sealed class AssignTaskRequestValidator
    : AbstractValidator<AssignTaskRequestDto>
{
    public AssignTaskRequestValidator()
    {
        RuleFor(request => request.AssignedToUserId)
            .GreaterThan(0)
            .WithMessage("Atanan kullanıcı ID değeri geçerli değildir.")
            .When(request => request.AssignedToUserId.HasValue);
    }
}