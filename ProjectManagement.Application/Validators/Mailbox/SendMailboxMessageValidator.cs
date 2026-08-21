using FluentValidation;
using ProjectManagement.Application.DTOs.Mailbox;
using ProjectManagement.Application.Mailbox;

namespace ProjectManagement.Application.Validators.Mailbox;

public sealed class SendMailboxMessageValidator
    : AbstractValidator<SendMailboxMessageDto>
{
    public SendMailboxMessageValidator()
    {
        RuleFor(request => request.RecipientUserIds)
            .NotEmpty()
            .WithMessage(
                "En az bir alıcı seçilmelidir.");

        RuleFor(request => request.RecipientUserIds.Count)
            .LessThanOrEqualTo(50)
            .WithMessage(
                "Bir mesaj en fazla 50 kullanıcıya gönderilebilir.");

        RuleFor(request => request.RecipientUserIds)
            .Must(HaveUniqueRecipientIds)
            .WithMessage(
                "Aynı kullanıcı alıcı listesine birden fazla kez eklenemez.");

        RuleForEach(request => request.RecipientUserIds)
            .GreaterThan(0)
            .WithMessage(
                "Alıcı kullanıcı kimliği geçersizdir.");

        RuleFor(request => request.Subject)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Mesaj konusu zorunludur.")
            .MaximumLength(
                MailboxFileConstants.MaximumSubjectLength)
            .WithMessage(
                $"Mesaj konusu en fazla {MailboxFileConstants.MaximumSubjectLength} karakter olabilir.");

        RuleFor(request => request.Body)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Mesaj içeriği zorunludur.")
            .MaximumLength(
                MailboxFileConstants.MaximumBodyLength)
            .WithMessage(
                $"Mesaj içeriği en fazla {MailboxFileConstants.MaximumBodyLength} karakter olabilir.");

        RuleFor(request => request.Attachments.Count)
            .LessThanOrEqualTo(
                MailboxFileConstants.MaximumAttachmentCount)
            .WithMessage(
                $"Bir mesaja en fazla {MailboxFileConstants.MaximumAttachmentCount} dosya eklenebilir.");

        RuleFor(request => request.Attachments)
            .Must(HaveValidTotalFileSize)
            .WithMessage(
                "Eklenen dosyaların toplam boyutu 200 MB'ı geçemez.");

        RuleForEach(request => request.Attachments)
            .SetValidator(
                new UploadedMailboxFileValidator());
    }

    private static bool HaveUniqueRecipientIds(
        IReadOnlyCollection<int> recipientUserIds)
    {
        return recipientUserIds
            .Distinct()
            .Count() == recipientUserIds.Count;
    }

    private static bool HaveValidTotalFileSize(
        IReadOnlyCollection<UploadedMailboxFileDto> files)
    {
        /*
         * long taşmasına karşı Aggregate yerine kontrollü
         * toplama yapıyoruz.
         */
        long totalSize = 0;

        foreach (var file in files)
        {
            if (file.Length < 0)
            {
                return false;
            }

            if (totalSize >
                MailboxFileConstants.MaximumTotalFileSize -
                file.Length)
            {
                return false;
            }

            totalSize += file.Length;
        }

        return totalSize <=
               MailboxFileConstants.MaximumTotalFileSize;
    }
}