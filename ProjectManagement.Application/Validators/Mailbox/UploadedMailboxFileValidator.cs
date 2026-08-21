using FluentValidation;
using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Application.Mailbox;

namespace ProjectManagement.Application.Validators.Mailbox;

public sealed class UploadedMailboxFileValidator
    : AbstractValidator<UploadedMailboxFileDto>
{
    public UploadedMailboxFileValidator()
    {
        RuleFor(file => file.FileName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Dosya adı boş olamaz.")
            .MaximumLength(255)
            .WithMessage(
                "Dosya adı en fazla 255 karakter olabilir.");

        RuleFor(file => file.Length)
            .GreaterThan(0)
            .WithMessage(
                "Boş dosya yüklenemez.")
            .LessThanOrEqualTo(
                MailboxFileConstants.MaximumFileSize)
            .WithMessage(
                "Bir dosyanın boyutu 200 MB'ı geçemez.");

        RuleFor(file => file.Content)
            .NotNull()
            .WithMessage(
                "Dosya içeriği bulunamadı.");

        RuleFor(file => file.FileName)
            .Must(HaveAllowedExtension)
            .WithMessage(
                "Yalnızca PDF, Word, ZIP, PNG, JPG ve JPEG dosyaları yüklenebilir.");

        RuleFor(file => file.ContentType)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Dosya içerik türü belirlenemedi.")
            .Must(HaveAllowedContentType)
            .WithMessage(
                "Dosyanın içerik türüne izin verilmiyor.");
    }

    private static bool HaveAllowedExtension(
        string fileName)
    {
        var extension =
            Path.GetExtension(fileName);

        return
            !string.IsNullOrWhiteSpace(extension) &&
            MailboxFileConstants.AllowedExtensions.Contains(
                extension);
    }

    private static bool HaveAllowedContentType(
        string contentType)
    {
        return MailboxFileConstants
            .AllowedContentTypes
            .Contains(contentType);
    }
}