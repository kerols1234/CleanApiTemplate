namespace CleanApi.Infrastructure.Persistence.Outbox;

/// <summary>
/// A domain event persisted in the same transaction as the state change that raised it, then
/// published asynchronously by the outbox processor. Guarantees at-least-once delivery even if the
/// process crashes after commit.
/// </summary>
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    /// <summary>The event's CLR type full name, used to rehydrate it for publishing.</summary>
    public string Type { get; set; } = default!;

    /// <summary>The JSON-serialized event payload.</summary>
    public string Content { get; set; } = default!;

    public DateTimeOffset OccurredOnUtc { get; set; }

    public DateTimeOffset? ProcessedOnUtc { get; set; }

    public int Attempts { get; set; }

    public string? Error { get; set; }
}
