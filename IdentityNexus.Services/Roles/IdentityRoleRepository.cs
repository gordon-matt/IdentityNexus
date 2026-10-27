using System.Security.Claims;
using IdentityNexus.Services.Tenancy;

namespace IdentityNexus.Services.Roles;

internal sealed class IdentityRoleRepository<TRole>(
    RoleManager<TRole> roleManager,
    IServiceProvider services,
    IEnumerable<ITenantRule<TRole>> tenants) : IRoleRepository
    where TRole : IdentityRole, new()
{
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        EnsureQueryable();
        return ScopedRoles().CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoleSummary>> ListAsync(CancellationToken cancellationToken = default)
    {
        EnsureQueryable();

        var roles = await ScopedRoles()
            .OrderBy(role => role.Name)
            .ToListAsync(cancellationToken);

        var summaries = new List<RoleSummary>(roles.Count);
        foreach (var role in roles)
        {
            summaries.Add(await ToSummaryAsync(role));
        }

        return summaries;
    }

    public async Task<RoleSummary?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await FindInTenantAsync(id);
        return role is null ? null : await ToSummaryAsync(role);
    }

    public async Task<(string? Id, IdentityResult Result)> CreateAsync(
        string name,
        IReadOnlyList<UserClaim> claims,
        CancellationToken cancellationToken = default)
    {
        var role = new TRole { Name = name.Trim() };
        Stamp(role);
        var created = await roleManager.CreateAsync(role);
        if (!created.Succeeded)
        {
            return (null, created);
        }

        var savedClaims = await SetClaimsAsync(role, claims);
        return (role.Id, savedClaims);
    }

    public async Task<IdentityResult> UpdateAsync(
        string id,
        string name,
        IReadOnlyList<UserClaim> claims,
        CancellationToken cancellationToken = default)
    {
        var role = await FindInTenantAsync(id);
        if (role is null)
        {
            return Missing();
        }

        var renamed = await roleManager.SetRoleNameAsync(role, name.Trim());
        if (!renamed.Succeeded)
        {
            return renamed;
        }

        var updated = await roleManager.UpdateAsync(role);
        return updated.Succeeded ? await SetClaimsAsync(role, claims) : updated;
    }

    public async Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var role = await FindInTenantAsync(id);
        return role is null ? Missing() : await roleManager.DeleteAsync(role);
    }

    private IQueryable<TRole> ScopedRoles() =>
        tenants.Aggregate(roleManager.Roles, (query, rule) => rule.Apply(query, services));

    private void Stamp(TRole role)
    {
        foreach (var rule in tenants)
        {
            rule.Assign(role, services);
        }
    }

    private async Task<TRole?> FindInTenantAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        return role is null || !tenants.All(rule => rule.Matches(role, services)) ? null : role;
    }

    private async Task<RoleSummary> ToSummaryAsync(TRole role)
    {
        var claims = await roleManager.GetClaimsAsync(role);
        return new RoleSummary(
            role.Id!,
            role.Name,
            claims
                .Select(claim => new UserClaim(claim.Type, claim.Value))
                .OrderBy(claim => claim.Type, StringComparer.Ordinal)
                .ThenBy(claim => claim.Value, StringComparer.Ordinal)
                .ToArray());
    }

    private async Task<IdentityResult> SetClaimsAsync(TRole role, IReadOnlyList<UserClaim> claims)
    {
        var desired = claims
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Type))
            .Select(claim => new UserClaim(claim.Type.Trim(), claim.Value.Trim()))
            .Distinct()
            .ToArray();

        var current = await roleManager.GetClaimsAsync(role);

        foreach (var claim in current.Where(claim => !desired.Any(item => item.Type == claim.Type && item.Value == claim.Value)))
        {
            var removed = await roleManager.RemoveClaimAsync(role, claim);
            if (!removed.Succeeded)
            {
                return removed;
            }
        }

        foreach (var claim in desired.Where(item => !current.Any(claim => claim.Type == item.Type && claim.Value == item.Value)))
        {
            var added = await roleManager.AddClaimAsync(role, new Claim(claim.Type, claim.Value));
            if (!added.Succeeded)
            {
                return added;
            }
        }

        return IdentityResult.Success;
    }

    private void EnsureQueryable()
    {
        if (!roleManager.SupportsQueryableRoles)
        {
            throw new InvalidOperationException(
                "IdentityNexus needs a queryable role store, such as the Entity Framework Core Identity store.");
        }
    }

    private static IdentityResult Missing() => IdentityResult.Failed(new IdentityError
    {
        Code = "RoleNotFound",
        Description = "That role no longer exists.",
    });
}