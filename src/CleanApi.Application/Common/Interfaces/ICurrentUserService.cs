namespace CleanApi.Application.Common.Interfaces;

/// <summary>Ambient information about the caller, resolved from the HTTP context.</summary>
public interface ICurrentUserService
{
    string? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    /// <summary>Returns the user id or throws if unauthenticated. Use after the auth pipeline.</summary>
    string GetRequiredUserId();
}
