using CleanApi.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace CleanApi.Api.Common;

/// <summary>
/// Single place that maps the Application <see cref="Result"/>/<see cref="Result{T}"/> to HTTP.
/// Controllers just <c>return result.ToActionResult();</c>. Failures are RFC 7807 ProblemDetails.
/// </summary>
public static class ApiResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        if (result.Value is FileDto file)
        {
            return new FileContentResult(file.Content, file.ContentType) { FileDownloadName = file.FileName };
        }

        return result.Status switch
        {
            ResultStatus.Created => new ObjectResult(Payload(result.Value, result.Message)) { StatusCode = StatusCodes.Status201Created },
            ResultStatus.NoContent => new NoContentResult(),
            _ => new OkObjectResult(Payload(result.Value, result.Message)),
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (!result.IsSuccess)
        {
            return Failure(result);
        }

        return result.Status == ResultStatus.NoContent
            ? new NoContentResult()
            : new OkObjectResult(new { message = result.Message });
    }

    private static object Payload<T>(T? value, string? message) => new { data = value, message };

    private static IActionResult Failure(Result result)
    {
        if (result.Status == ResultStatus.Invalid)
        {
            var validationProblem = new ValidationProblemDetails(
                result.ValidationErrors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
            };
            return new ObjectResult(validationProblem) { StatusCode = StatusCodes.Status400BadRequest };
        }

        var statusCode = MapStatus(result.Status);
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = ReasonPhrase(result.Status),
            Detail = result.Message,
        };
        return new ObjectResult(problem) { StatusCode = statusCode };
    }

    private static int MapStatus(ResultStatus status) => status switch
    {
        ResultStatus.NotFound => StatusCodes.Status404NotFound,
        ResultStatus.Conflict => StatusCodes.Status409Conflict,
        ResultStatus.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultStatus.Forbidden => StatusCodes.Status403Forbidden,
        ResultStatus.Invalid => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static string ReasonPhrase(ResultStatus status) => status switch
    {
        ResultStatus.NotFound => "Resource not found",
        ResultStatus.Conflict => "Conflict",
        ResultStatus.Unauthorized => "Unauthorized",
        ResultStatus.Forbidden => "Forbidden",
        _ => "An error occurred",
    };
}
