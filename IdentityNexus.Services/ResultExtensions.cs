namespace IdentityNexus.Services;

public static class ResultExtensions
{
    public static bool IsSuccessStatus(this IResult result) =>
        result.Status is ResultStatus.Ok or ResultStatus.Created or ResultStatus.NoContent;

    /// <summary>
    /// Re-shapes a failed result as <see cref="Result{TTo}"/>, keeping the status and messages.
    /// A success has no value to carry, so it throws.
    /// </summary>
    public static Result<TTo> To<TTo>(this IResult result)
    {
        if (result.IsSuccessStatus())
        {
            throw new InvalidOperationException(
                $"{nameof(To)} propagates failures. A successful result has no value to convert into Result<{typeof(TTo).Name}>.");
        }

        string[] errors = result.Errors?.ToArray() ?? [];

        return result.Status switch
        {
            ResultStatus.NotFound => Result<TTo>.NotFound(errors),
            ResultStatus.Forbidden => Result<TTo>.Forbidden(errors),
            ResultStatus.Unauthorized => Result<TTo>.Unauthorized(errors),
            ResultStatus.Invalid => Result<TTo>.Invalid([.. result.ValidationErrors ?? []]),
            ResultStatus.Conflict => Result<TTo>.Conflict(errors),
            ResultStatus.Unavailable => Result<TTo>.Unavailable(errors),
            ResultStatus.CriticalError => Result<TTo>.CriticalError(errors),
            _ => Result<TTo>.Error(new ErrorList(errors)),
        };
    }

    public static IReadOnlyList<string> GetDisplayMessages(this IResult result)
    {
        var messages = new List<string>();

        if (result.ValidationErrors is not null)
        {
            messages.AddRange(result.ValidationErrors
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))!);
        }

        if (result.Errors is not null)
        {
            messages.AddRange(result.Errors.Where(error => !string.IsNullOrWhiteSpace(error)));
        }

        if (messages.Count == 0 && !result.IsSuccessStatus())
        {
            messages.Add(result.Status switch
            {
                ResultStatus.NotFound => "That record no longer exists.",
                ResultStatus.Forbidden => "You are not allowed to do that.",
                ResultStatus.Conflict => "That conflicts with something that already exists.",
                ResultStatus.Invalid => "Some of the values supplied were not valid.",
                _ => "Something went wrong. Please try again.",
            });
        }

        return messages;
    }
}