using N10.Services.Results;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace N10.WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this N10.Services.Results.Result<T> result) =>
        result.Match<IResult>(
            onSuccess: value => HttpResults.Ok(value),
            onFailure: errors => errors[0].Type switch
            {
                ErrorType.Validation => HttpResults.BadRequest(
                    new { errors = errors.Select(e => new { e.Code, e.Message }) }),
                ErrorType.NotFound => HttpResults.NotFound(
                    new { errors[0].Code, errors[0].Message }),
                ErrorType.Conflict => HttpResults.Conflict(
                    new { errors[0].Code, errors[0].Message }),
                ErrorType.Unauthorized => HttpResults.Unauthorized(),
                ErrorType.Unexpected => HttpResults.InternalServerError(),
                _ => HttpResults.InternalServerError()
            });

    public static IResult ToHttpResult(this N10.Services.Results.Result result) =>
        result.Match<IResult>(
            onSuccess: HttpResults.NoContent,
            onFailure: errors => errors[0].Type switch
            {
                ErrorType.Validation => HttpResults.BadRequest(
                    new { errors = errors.Select(e => new { e.Code, e.Message }) }),
                ErrorType.NotFound => HttpResults.NotFound(
                    new { errors[0].Code, errors[0].Message }),
                ErrorType.Conflict => HttpResults.Conflict(
                    new { errors[0].Code, errors[0].Message }),
                ErrorType.Unauthorized => HttpResults.Unauthorized(),
                ErrorType.Unexpected => HttpResults.InternalServerError(),
                _ => HttpResults.InternalServerError()
            });
}
