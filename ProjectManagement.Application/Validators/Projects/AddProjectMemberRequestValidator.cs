using FluentValidation;
using ProjectManagement.Application.DTOs.ProjectMembers;

namespace ProjectManagement.Application.Validators.ProjectMembers;
public sealed class AddProjectMemberRequestValidator
    : AbstractValidator<AddProjectMemberRequestDto>
{
    public AddProjectMemberRequestValidator()
    {
        RuleFor(request => request.UserId)
            .GreaterThan(0)
            .WithMessage("Projeye eklenecek kullanıcı ID değeri geçerli değildir.");

        RuleFor(request => request.Role)
            .IsInEnum()
            .WithMessage("Geçerli bir proje üyelik rolü seçilmelidir.");
    }
}