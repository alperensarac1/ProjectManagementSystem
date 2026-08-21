using FluentValidation;
using ProjectManagement.Application.DTOs.ProjectMembers;

namespace ProjectManagement.Application.Validators.ProjectMembers;

/// <summary>
/// Proje üyesi rol güncelleme isteğini doğrular.
/// </summary>
public sealed class UpdateProjectMemberRequestValidator
    : AbstractValidator<UpdateProjectMemberRequestDto>
{
    public UpdateProjectMemberRequestValidator()
    {
        RuleFor(request => request.Role)
            .IsInEnum()
            .WithMessage("Geçerli bir proje üyelik rolü seçilmelidir.");
    }
}