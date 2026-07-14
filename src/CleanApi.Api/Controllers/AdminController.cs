using Asp.Versioning;
using CleanApi.Api.Authorization;
using CleanApi.Application.Common.Interfaces;
using CleanApi.Domain.Authorization;
using CleanApi.Infrastructure.Jobs;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Api.Controllers;

/// <summary>Demonstrates notifications, email, and both job mechanisms (Hangfire + in-process queue).</summary>
[ApiVersion("1.0")]
[Authorize]
public sealed class AdminController(
    INotificationService notifications,
    IEmailService email,
    IBackgroundTaskQueue taskQueue)
    : ApiControllerBase
{
    [HttpPost("notifications/push")]
    [HasPermission(Permissions.Admin.SendNotifications)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> SendPush([FromBody] PushNotificationRequest request, CancellationToken cancellationToken)
    {
        var invalidTokens = await notifications.SendToTokensAsync(
            request.DeviceTokens,
            new PushNotification(request.Title, request.Body),
            cancellationToken);

        return Ok(new { message = "Notification dispatched (no-op if Firebase is not configured).", invalidTokens });
    }

    [HttpPost("notifications/email")]
    [HasPermission(Permissions.Admin.SendNotifications)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request, CancellationToken cancellationToken)
    {
        await email.SendAsync(new EmailMessage([request.To], request.Subject, request.HtmlBody), cancellationToken);
        return Accepted(new { message = "Email sent." });
    }

    [HttpPost("jobs/low-stock-report")]
    [HasPermission(Permissions.Admin.ManageUsers)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public IActionResult EnqueueLowStockReport()
    {
        // Hangfire fire-and-forget (survives restarts when backed by SQL Server storage).
        var jobId = BackgroundJob.Enqueue<SampleRecurringJob>(job => job.ReportLowStockAsync());
        return Accepted(new { message = "Hangfire job enqueued.", jobId });
    }

    [HttpPost("jobs/background-task")]
    [HasPermission(Permissions.Admin.ManageUsers)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> EnqueueBackgroundTask()
    {
        // In-process Channel-based queue (does NOT survive restarts).
        await taskQueue.EnqueueAsync(async (serviceProvider, cancellationToken) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<AdminController>>();
            logger.LogInformation("In-process background task executed.");
            await Task.Delay(100, cancellationToken);
        });

        return Accepted(new { message = "Background task enqueued." });
    }
}

public sealed record PushNotificationRequest(IReadOnlyCollection<string> DeviceTokens, string Title, string Body);

public sealed record EmailRequest(string To, string Subject, string HtmlBody);
