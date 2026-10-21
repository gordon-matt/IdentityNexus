namespace IdentityNexus.Services.Roles;

public sealed class RoleService(ILogger<RoleService> logger, IRoleRepository roles) : IRoleService
{
    public Task<Result<IReadOnlyList<RoleSummary>>> ListAsync(CancellationToken cancellationToken = default) =>
        RunAsync("list roles", async () =>
            Result<IReadOnlyList<RoleSummary>>.Success(await roles.ListAsync(cancellationToken)));

    public Task<Result<RoleSummary>> GetAsync(string id, CancellationToken cancellationToken = default) =>
        RunAsync("load the role", async () =>
        {
            var role = await roles.FindAsync(id, cancellationToken);
            return role is null
                ? Result<RoleSummary>.NotFound("That role no longer exists.")
                : Result<RoleSummary>.Success(role);
        });

    public Task<Result<RoleSummary>> SaveAsync(SaveRoleRequest request, CancellationToken cancellationToken = default) =>
        RunAsync("save the role", async () =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Result<RoleSummary>.Invalid(new ValidationError
                {
                    Identifier = "Name",
                    ErrorMessage = "A role name is required.",
                });
            }

            if (request.Name.Trim().Length > 256)
            {
                return Result<RoleSummary>.Invalid(new ValidationError
                {
                    Identifier = "Name",
                    ErrorMessage = "Role names are limited to 256 characters.",
                });
            }

            string? id = request.Id;
            IdentityResult identity;

            if (string.IsNullOrWhiteSpace(id))
            {
                var created = await roles.CreateAsync(request.Name, request.Claims, cancellationToken);
                id = created.Id;
                identity = created.Result;
            }
            else
            {
                identity = await roles.UpdateAsync(id, request.Name, request.Claims, cancellationToken);
            }

            if (!identity.Succeeded)
            {
                return identity.Errors.Any(error => error.Code == "RoleNotFound")
                    ? Result<RoleSummary>.NotFound("That role no longer exists.")
                    : IdentityResultMapper.ToResult<RoleSummary>(identity);
            }

            var saved = await roles.FindAsync(id!, cancellationToken);
            return saved is null
                ? Result<RoleSummary>.Error("The role was saved but could not be loaded.")
                : string.IsNullOrWhiteSpace(request.Id)
                    ? Result<RoleSummary>.Created(saved)
                    : Result<RoleSummary>.Success(saved);
        });

    public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        RunAsync("delete the role", async () =>
        {
            var identity = await roles.DeleteAsync(id, cancellationToken);
            if (identity.Succeeded)
            {
                return Result.Success();
            }

            return identity.Errors.Any(error => error.Code == "RoleNotFound")
                ? Result.NotFound("That role no longer exists.")
                : IdentityResultMapper.ToResult(identity);
        });

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
}
