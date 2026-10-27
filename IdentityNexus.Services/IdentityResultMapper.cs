namespace IdentityNexus.Services;

internal static class IdentityResultMapper
{
    private static readonly HashSet<string> ConflictCodes = new(StringComparer.Ordinal)
    {
        "DuplicateUserName",
        "DuplicateEmail",
        "DuplicateRoleName",
    };

    public static Result ToResult(IdentityResult identity)
    {
        if (identity.Succeeded)
        {
            return Result.Success();
        }

        var errors = identity.Errors
            .Select(error => string.IsNullOrWhiteSpace(error.Description) ? error.Code : error.Description)
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .ToArray();

        if (identity.Errors.Any(error => error.Code is "UserNotFound" or "RoleNotFound"))
        {
            return Result.NotFound(errors);
        }

        if (identity.Errors.Any(error => ConflictCodes.Contains(error.Code)))
        {
            return Result.Conflict(errors);
        }

        return Result.Invalid(identity.Errors.Select(error => new ValidationError
        {
            Identifier = error.Code,
            ErrorMessage = string.IsNullOrWhiteSpace(error.Description) ? error.Code : error.Description,
        }).ToArray());
    }

    public static Result<T> ToResult<T>(IdentityResult identity) => ToResult(identity).To<T>();
}