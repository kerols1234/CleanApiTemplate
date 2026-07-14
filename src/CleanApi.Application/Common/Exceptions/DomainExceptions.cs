namespace CleanApi.Application.Common.Exceptions;

/// <summary>Requested entity does not exist. Mapped to HTTP 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>Authenticated user lacks access to the resource. Mapped to HTTP 403.</summary>
public sealed class ForbiddenAccessException(string message = "Access denied.") : Exception(message);

/// <summary>Request conflicts with current state (duplicate, concurrency, invariant). Mapped to HTTP 409.</summary>
public sealed class ConflictException(string message) : Exception(message);
