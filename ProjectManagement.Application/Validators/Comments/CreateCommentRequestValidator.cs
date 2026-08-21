using FluentValidation;
using ProjectManagement.Application.DTOs.Comments;

namespace ProjectManagement.Application.Validators.Comments;

public sealed class CreateCommentRequestValidator
    : AbstractValidator<CreateCommentRequestDto>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(request => request.Content)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Yorum içeriği zorunludur.")
            .MaximumLength(5000)
            .WithMessage("Yorum içeriği en fazla 5000 karakter olabilir.");
    }
}