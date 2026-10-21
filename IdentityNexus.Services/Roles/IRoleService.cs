namespace IdentityNexus.Services.Roles;

public interface IRoleService
{
    Task<Result<IReadOnlyList<RoleSummary>>> ListAsync(CancellationToken cancellationToken = default);

    Task<Result<RoleSummary>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<RoleSummary>> SaveAsync(SaveRoleRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
