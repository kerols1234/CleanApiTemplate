namespace CleanApi.Domain.Common;

/// <summary>
/// Entities implementing this are stamped with audit metadata automatically by the
/// <c>AuditableEntityInterceptor</c> on save. Never set these fields by hand.
/// </summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAt { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset? LastModifiedAt { get; set; }
    string? LastModifiedBy { get; set; }
}
