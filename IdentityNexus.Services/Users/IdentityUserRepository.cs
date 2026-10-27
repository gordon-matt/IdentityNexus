using IdentityNexus.Services.Tenancy;

namespace IdentityNexus.Services.Users;

internal sealed class IdentityUserRepository<TUser>(
    UserManager<TUser> userManager,
    IServiceProvider services,
    IEnumerable<ITenantRule<TUser>> tenants) : IUserRepository
    where TUser : IdentityUser, new()
{
    public async Task<(int Users, int LockedOut, int UnconfirmedEmail)> CountAsync(CancellationToken cancellationToken = default)
    {
        EnsureQueryable();

        DateTimeOffset now = DateTimeOffset.UtcNow;
        int users = await ScopedUsers().CountAsync(cancellationToken);
        int lockedOut = (await LockedIdsAsync(now, cancellationToken)).Count;
        int unconfirmed = await ScopedUsers().CountAsync(user => !user.EmailConfirmed, cancellationToken);

        return (users, lockedOut, unconfirmed);
    }

    public async Task<UserPage> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default)
    {
        EnsureQueryable();

        IQueryable<TUser> users = ScopedUsers();

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            var inRole = await userManager.GetUsersInRoleAsync(query.Role.Trim());
            var ids = inRole.Where(InTenant).Select(user => user.Id).ToHashSet(StringComparer.Ordinal);
            users = users.Where(user => ids.Contains(user.Id));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string needle = query.Search.Trim().ToLower();
            users = users.Where(user =>
                (user.UserName != null && user.UserName.ToLower().Contains(needle)) ||
                (user.Email != null && user.Email.ToLower().Contains(needle)) ||
                (user.PhoneNumber != null && user.PhoneNumber.ToLower().Contains(needle)));
        }

        if (query.LockedOutOnly)
        {
            var lockedIds = await LockedIdsAsync(DateTimeOffset.UtcNow, cancellationToken);
            users = users.Where(user => lockedIds.Contains(user.Id));
        }

        if (query.UnconfirmedOnly)
        {
            users = users.Where(user => !user.EmailConfirmed);
        }

        int total = await users.CountAsync(cancellationToken);
        int pageSize = query.PageSize;
        int page = query.Page;

        var pageItems = await users
            .OrderBy(user => user.UserName)
            .ThenBy(user => user.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var summaries = new List<UserSummary>(pageItems.Count);
        foreach (var user in pageItems)
        {
            var roles = await userManager.GetRolesAsync(user);
            summaries.Add(ToSummary(user, roles));
        }

        return new UserPage(summaries, total, page, pageSize);
    }

    public async Task<UserDetail?> FindAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(id);
        return user is null ? null : await ToDetailAsync(user);
    }

    public async Task<(string? Id, IdentityResult Result)> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new TUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
        };
        Stamp(user);

        var created = await userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            return (null, created);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var phone = await userManager.SetPhoneNumberAsync(user, request.PhoneNumber.Trim());
            if (!phone.Succeeded)
            {
                return (user.Id, phone);
            }
        }

        var roles = NormalizeRoles(request.Roles);
        if (roles.Count > 0)
        {
            var assigned = await userManager.AddToRolesAsync(user, roles);
            if (!assigned.Succeeded)
            {
                return (user.Id, assigned);
            }
        }

        return (user.Id, IdentityResult.Success);
    }

    public async Task<IdentityResult> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(request.Id);
        if (user is null)
        {
            return Missing();
        }

        var userName = await userManager.SetUserNameAsync(user, request.UserName.Trim());
        if (!userName.Succeeded)
        {
            return userName;
        }

        var email = await userManager.SetEmailAsync(user, request.Email.Trim());
        if (!email.Succeeded)
        {
            return email;
        }

        string? phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        var phone = await userManager.SetPhoneNumberAsync(user, phoneNumber);
        if (!phone.Succeeded)
        {
            return phone;
        }

        user.EmailConfirmed = request.EmailConfirmed;
        user.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
        var confirmed = await userManager.UpdateAsync(user);
        if (!confirmed.Succeeded)
        {
            return confirmed;
        }

        var twoFactor = await userManager.SetTwoFactorEnabledAsync(user, request.TwoFactorEnabled);
        if (!twoFactor.Succeeded)
        {
            return twoFactor;
        }

        var lockoutEnabled = await userManager.SetLockoutEnabledAsync(user, request.LockoutEnabled);
        if (!lockoutEnabled.Succeeded)
        {
            return lockoutEnabled;
        }

        var lockoutEnd = await userManager.SetLockoutEndDateAsync(user, request.LockoutEnd);
        if (!lockoutEnd.Succeeded)
        {
            return lockoutEnd;
        }

        if (request.LockoutEnd is null)
        {
            var reset = await userManager.ResetAccessFailedCountAsync(user);
            if (!reset.Succeeded)
            {
                return reset;
            }
        }

        var roles = await SetRolesAsync(user, request.Roles);
        if (!roles.Succeeded)
        {
            return roles;
        }

        return await SetClaimsAsync(user, request.Claims);
    }

    public async Task<IdentityResult> SetPasswordAsync(string id, string password, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(id);
        if (user is null)
        {
            return Missing();
        }

        if (await userManager.HasPasswordAsync(user))
        {
            var removed = await userManager.RemovePasswordAsync(user);
            if (!removed.Succeeded)
            {
                return removed;
            }
        }

        return await userManager.AddPasswordAsync(user, password);
    }

    public async Task<IdentityResult> RemoveLoginAsync(string id, string loginProvider, string providerKey, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(id);
        return user is null
            ? Missing()
            : await userManager.RemoveLoginAsync(user, loginProvider, providerKey);
    }

    public async Task<IdentityResult> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(id);
        if (user is null)
        {
            return Missing();
        }

        var disabled = await userManager.SetTwoFactorEnabledAsync(user, false);
        return !disabled.Succeeded
            ? disabled
            : await userManager.ResetAuthenticatorKeyAsync(user);
    }

    public async Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await FindInTenantAsync(id);
        return user is null ? Missing() : await userManager.DeleteAsync(user);
    }

    private async Task<IdentityResult> SetRolesAsync(TUser user, IReadOnlyList<string> roles)
    {
        var desired = NormalizeRoles(roles);
        var current = await userManager.GetRolesAsync(user);

        var desiredSet = desired.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var currentSet = current.ToHashSet(StringComparer.OrdinalIgnoreCase);

        string[] remove = [.. current.Where(role => !desiredSet.Contains(role))];
        string[] add = [.. desired.Where(role => !currentSet.Contains(role))];

        if (remove.Length > 0)
        {
            var removed = await userManager.RemoveFromRolesAsync(user, remove);
            if (!removed.Succeeded)
            {
                return removed;
            }
        }

        return add.Length == 0
            ? IdentityResult.Success
            : await userManager.AddToRolesAsync(user, add);
    }

    private async Task<IdentityResult> SetClaimsAsync(TUser user, IReadOnlyList<UserClaim> claims)
    {
        var desired = claims
            .Where(claim => !string.IsNullOrWhiteSpace(claim.Type))
            .Select(claim => new UserClaim(claim.Type.Trim(), claim.Value.Trim()))
            .Distinct()
            .ToArray();

        var current = await userManager.GetClaimsAsync(user);

        foreach (var claim in current.Where(claim => !desired.Any(item => item.Type == claim.Type && item.Value == claim.Value)))
        {
            var removed = await userManager.RemoveClaimAsync(user, claim);
            if (!removed.Succeeded)
            {
                return removed;
            }
        }

        foreach (var claim in desired.Where(item => !current.Any(claim => claim.Type == item.Type && claim.Value == item.Value)))
        {
            var added = await userManager.AddClaimAsync(user, new System.Security.Claims.Claim(claim.Type, claim.Value));
            if (!added.Succeeded)
            {
                return added;
            }
        }

        return IdentityResult.Success;
    }

    private async Task<UserDetail> ToDetailAsync(TUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);
        var logins = await userManager.GetLoginsAsync(user);

        return new UserDetail(
            user.Id,
            user.UserName,
            user.Email,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.LockoutEnabled,
            user.LockoutEnd,
            user.TwoFactorEnabled,
            user.AccessFailedCount,
            roles.OrderBy(role => role, StringComparer.OrdinalIgnoreCase).ToArray(),
            claims.Select(claim => new UserClaim(claim.Type, claim.Value)).OrderBy(claim => claim.Type, StringComparer.Ordinal).ToArray(),
            logins.Select(login => new UserLogin(login.LoginProvider, login.ProviderKey, login.ProviderDisplayName)).ToArray());
    }

    private static UserSummary ToSummary(TUser user, IList<string> roles) => new(
        user.Id,
        user.UserName,
        user.Email,
        user.EmailConfirmed,
        user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
        roles.OrderBy(role => role, StringComparer.OrdinalIgnoreCase).ToArray());

    private static List<string> NormalizeRoles(IReadOnlyList<string> roles) =>
        roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<List<string>> LockedIdsAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        // DateTimeOffset comparisons do not translate on every provider (SQLite in particular),
        // so the cutoff is applied in memory to the rows that actually have a lockout end.
        var rows = await ScopedUsers()
            .Where(user => user.LockoutEnd != null)
            .Select(user => new { user.Id, user.LockoutEnd })
            .ToListAsync(cancellationToken);

        return rows
            .Where(user => user.LockoutEnd > now)
            .Select(user => user.Id)
            .ToList();
    }

    private IQueryable<TUser> ScopedUsers() =>
        tenants.Aggregate(userManager.Users, (query, rule) => rule.Apply(query, services));

    private bool InTenant(TUser user) => tenants.All(rule => rule.Matches(user, services));

    private void Stamp(TUser user)
    {
        foreach (var rule in tenants)
        {
            rule.Assign(user, services);
        }
    }

    private async Task<TUser?> FindInTenantAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        return user is null || !InTenant(user) ? null : user;
    }

    private void EnsureQueryable()
    {
        if (!userManager.SupportsQueryableUsers)
        {
            throw new InvalidOperationException(
                "IdentityNexus needs a queryable user store, such as the Entity Framework Core Identity store.");
        }
    }

    private static IdentityResult Missing() => IdentityResult.Failed(new IdentityError
    {
        Code = "UserNotFound",
        Description = "That user no longer exists.",
    });
}