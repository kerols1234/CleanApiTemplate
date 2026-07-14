using CleanApi.Application.Common.Interfaces;
using CleanApi.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace CleanApi.Infrastructure.Services;

/// <summary>MailKit-based email sender. Opens a fresh SMTP connection per send (no startup connection).</summary>
public sealed class EmailService(EmailSettings settings, ILogger<EmailService> logger) : IEmailService
{
    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        foreach (var recipient in message.To)
        {
            mime.To.Add(MailboxAddress.Parse(recipient));
        }

        mime.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message.HtmlBody };
        if (message.Attachments is not null)
        {
            foreach (var attachment in message.Attachments)
            {
                bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
            }
        }

        mime.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = settings.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
        await client.ConnectAsync(settings.Host, settings.Port, socketOptions, cancellationToken);

        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            await client.AuthenticateAsync(settings.Username, settings.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email sent to {Recipients}", string.Join(", ", message.To));
    }
}
