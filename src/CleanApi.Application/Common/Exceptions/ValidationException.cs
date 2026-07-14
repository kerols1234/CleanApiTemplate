using FluentValidation.Results;

namespace CleanApi.Application.Common.Exceptions;

/// <summary>
/// Thrown by the validation pipeline behavior when FluentValidation fails. Carries per-field
/// errors; the API's exception handler turns it into an RFC 7807 400 (ValidationProblemDetails).
/// Decoupled from FluentValidation's own exception so the API never references FluentValidation.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.Distinct().ToArray());
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
