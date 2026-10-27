using System.Security.Claims;
using IdentityNexus.Services.Roles;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;

namespace IdentityNexus.Services.Users;

public sealed class UserService(
    ILogger<UserService> logger,
    IOptions<IdentityNexusOptions> options,
    AuthenticationStateProvider authentication,
    IUserRepository users,
    IRoleRepository roles) : IUserService
{
    private const int MaxPageSize = 100;

    public Task<Result<IdentityOverview>> GetOverviewAsync(CancellationToken cancellationToken = default) =>
        RunAsync("load the overview", async () =>
        {
            var counts = await users.CountAsync(cancellationToken);
            int roleCount = await roles.CountAsync(cancellationToken);
            return Result<IdentityOverview>.Success(new IdentityOverview(counts.Users, roleCount, counts.LockedOut, counts.UnconfirmedEmail));
        });

    public Task<Result<UserPage>> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default) =>
        RunAsync("search users", async () =>
        {
            int pageSize = Math.Clamp(query.PageSize <= 0 ? options.Value.PageSize : query.PageSize, 1, MaxPageSize);
            int page = Math.Max(1, query.Page);

            var pageResult = await users.SearchAsync(query with { Page = page, PageSize = pageSize }, cancellationToken);
            return Result<UserPage>.Success(pageResult);
        });

    public Task<Result<UserDetail>> GetAsync(string id, CancellationToken cancellationToken = default) =>
        RunAsync("load the user", async () =>
        {
            var user = await users.FindAsync(id, cancellationToken);
            return user is null
                ? Result<UserDetail>.NotFound("That user no longer exists.")
                : Result<UserDetail>.Success(user);
        });

    public Task<Result<UserDetail>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default) =>
        RunAsync("create the user", async () =>
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return Result<UserDetail>.Invalid(Required("UserName", "A user name is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return Result<UserDetail>.Invalid(Required("Email", "An email address is required."));
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Result<UserDetail>.Invalid(Required("Password", "A password is required."));
            }

            var (id, identity) = await users.CreateAsync(request, cancellationToken);
            if (!identity.Succeeded)
            {
                var failed = IdentityResultMapper.ToResult<UserDetail>(identity);
                return id is null
                    ? failed
                    : Result<UserDetail>.Invalid(new ValidationError
                    {
                        Identifier = "Roles",
                        ErrorMessage = "The user was created, but a follow-up change failed. " + FirstMessage(failed),
                    });
            }

            var created = await users.FindAsync(id!, cancellationToken);
            return created is null
                ? Result<UserDetail>.Error("The user was created but could not be loaded.")
                : Result<UserDetail>.Created(created);
        });

    public Task<Result<UserDetail>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default) =>
        RunAsync("update the user", async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Id))
            {
                return Result<UserDetail>.NotFound("That user no longer exists.");
            }

            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Email))
            {
                return Result<UserDetail>.Invalid(Required("UserName", "A user name and email address are required."));
            }

            string? currentId = await CurrentUserIdAsync();
            if (string.Equals(currentId, request.Id, StringComparison.Ordinal)
                && request.LockoutEnd is { } end
                && end > DateTimeOffset.UtcNow)
            {
                return Result<UserDetail>.Conflict("You cannot lock the account you are signed in with.");
            }

            var identity = await users.UpdateAsync(request, cancellationToken);
            if (!identity.Succeeded)
            {
                return NotFoundOr(identity);
            }

            var updated = await users.FindAsync(request.Id, cancellationToken);
            return updated is null
                ? Result<UserDetail>.NotFound("That user no longer exists.")
                : Result<UserDetail>.Success(updated);
        });

    public Task<Result> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default) =>
        RunAsync("set the password", async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Result.Invalid(Required("Password", "A password is required."));
            }

            var identity = await users.SetPasswordAsync(request.Id, request.Password, cancellationToken);
            return identity.Succeeded ? Result.Success() : IdentityResultMapper.ToResult(identity);
        });

    public Task<Result> RemoveLoginAsync(RemoveLoginRequest request, CancellationToken cancellationToken = default) =>
        RunAsync("remove the external login", async () =>
        {
            var identity = await users.RemoveLoginAsync(request.Id, request.LoginProvider, request.ProviderKey, cancellationToken);
            return identity.Succeeded ? Result.Success() : IdentityResultMapper.ToResult(identity);
        });

    public Task<Result> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default) =>
        RunAsync("reset the authenticator", async () =>
        {
            var identity = await users.ResetAuthenticatorAsync(id, cancellationToken);
            return identity.Succeeded ? Result.Success() : IdentityResultMapper.ToResult(identity);
        });

    public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        RunAsync("delete the user", async () =>
        {
            string? currentId = await CurrentUserIdAsync();
            if (string.Equals(currentId, id, StringComparison.Ordinal))
            {
                return Result.Conflict("You cannot delete the account you are signed in with.");
            }

            var identity = await users.DeleteAsync(id, cancellationToken);
            if (identity.Succeeded)
            {
                return Result.Success();
            }

            return identity.Errors.Any(error => error.Code == "UserNotFound")
                ? Result.NotFound("That user no longer exists.")
                : IdentityResultMapper.ToResult(identity);
        });

    private async Task<string?> CurrentUserIdAsync()
    {
        var state = await authentication.GetAuthenticationStateAsync();
        return state.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private static Result<UserDetail> NotFoundOr(IdentityResult identity) =>
        identity.Errors.Any(error => error.Code == "UserNotFound")
            ? Result<UserDetail>.NotFound("That user no longer exists.")
            : IdentityResultMapper.ToResult<UserDetail>(identity);

    private async Task<Result<T>> RunAsync<T>(string action, Func<Task<Result<T>>> work)
    {
        try
        {
            return await work();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not {Action}.", action);
            return Result<T>.Error($"Could not {action}.");
        }
    }

    private async Task<Result> RunAsync(string action, Func<Task<Result>> work)
    {
        try
        {
            return await work();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not {Action}.", action);
            return Result.Error($"Could not {action}.");
        }
    }

    private static ValidationError Required(string field, string message) => new()
    {
        Identifier = field,
        ErrorMessage = message,
    };

    private static string FirstMessage(IResult result) =>
        result.GetDisplayMessages().FirstOrDefault() ?? "Check the user and try again.";
}