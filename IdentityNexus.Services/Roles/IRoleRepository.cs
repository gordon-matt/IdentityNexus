namespace IdentityNexus.Services.Roles;

public interface IRoleRepository
{
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleSummary>> ListAsync(CancellationToken cancellationToken = default);

    Task<RoleSummary?> FindAsync(string id, CancellationToken cancellationToken = default);

    Task<(string? Id, IdentityResult Result)> CreateAsync(string name, IReadOnlyList<UserClaim> claims, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateAsync(string id, string name, IReadOnlyList<UserClaim> claims, CancellationToken cancellationToken = default);

    Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
