namespace CleanApi.Application.Common.Interfaces;

public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType);

public sealed record EmailMessage(
    IReadOnlyCollection<string> To,
    string Subject,
    string HtmlBody,
    IReadOnlyCollection<EmailAttachment>? Attachments = null);

/// <summary>Sends transactional email (implemented with MailKit in Infrastructure).</summary>
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
