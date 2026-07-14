using System.Text.Json.Serialization;

namespace CleanApi.Application.Common.Models;

/// <summary>Outcome category of a <see cref="Result"/>, mapped to an HTTP status by the API layer.</summary>
public enum ResultStatus
{
    Ok,
    Created,
    NoContent,
    Invalid,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    Error,
}

/// <summary>
/// Non-generic operation result. Handlers return this (or <see cref="Result{T}"/>) instead of
/// throwing for expected failures; the API translates it to an <c>IActionResult</c> in one place.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    public ResultStatus Status { get; }

    public string? Message { get; }

    /// <summary>Validation errors keyed by field. Populated only when <see cref="Status"/> is Invalid.</summary>
    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }

    protected Result(bool isSuccess, ResultStatus status, string? message, IReadOnlyDictionary<string, string[]>? validationErrors = null)
    {
        IsSuccess = isSuccess;
        Status = status;
        Message = message;
        ValidationErrors = validationErrors ?? new Dictionary<string, string[]>();
    }

    public static Result Success(string? message = null) => new(true, ResultStatus.Ok, message);

    public static Result<T> Success<T>(T value, string? message = null) => Result<T>.Ok(value, message);

    public static Result<T> Created<T>(T value, string? message = null) =>
        new(value, true, ResultStatus.Created, message);

    public static Result NoContent() => new(true, ResultStatus.NoContent, null);

    public static Result NotFound(string message) => new(false, ResultStatus.NotFound, message);

    public static Result<T> NotFound<T>(string message) => new(default, false, ResultStatus.NotFound, message);

    public static Result Conflict(string message) => new(false, ResultStatus.Conflict, message);

    public static Result<T> Conflict<T>(string message) => new(default, false, ResultStatus.Conflict, message);

    public static Result Forbidden(string message = "You do not have access to this resource.") =>
        new(false, ResultStatus.Forbidden, message);

    public static Result Unauthorized(string message = "Authentication is required.") =>
        new(false, ResultStatus.Unauthorized, message);

    public static Result Error(string message) => new(false, ResultStatus.Error, message);

    public static Result<T> Error<T>(string message) => new(default, false, ResultStatus.Error, message);

    public static Result Invalid(IReadOnlyDictionary<string, string[]> errors) =>
        new(false, ResultStatus.Invalid, "One or more validation errors occurred.", errors);
}

/// <summary>Operation result carrying a value on success.</summary>
public sealed class Result<T> : Result
{
    public T? Value { get; }

    [JsonConstructor]
    public Result(T? value, bool isSuccess, ResultStatus status, string? message, IReadOnlyDictionary<string, string[]>? validationErrors = null)
        : base(isSuccess, status, message, validationErrors)
    {
        Value = value;
    }

    public static Result<T> Ok(T value, string? message = null) => new(value, true, ResultStatus.Ok, message);

    // Allow handlers to `return value;` directly for the common success path.
    public static implicit operator Result<T>(T value) => Ok(value);
}
