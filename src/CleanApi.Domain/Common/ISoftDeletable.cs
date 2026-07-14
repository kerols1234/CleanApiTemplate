namespace CleanApi.Domain.Common;

/// <summary>
/// Entities implementing this are soft-deleted: a global query filter hides rows where
/// <see cref="IsDeleted"/> is true, and deletes are converted to updates by the DbContext.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTimeOffset? DeletedAt { get; set; }
}
