namespace IdentityNexus.Services.Models;

public sealed record IdentityOverview(int Users, int Roles, int LockedOut, int UnconfirmedEmail);

public sealed record UserListQuery
{
    public string? Search { get; init; }

    public string? Role { get; init; }

    public bool LockedOutOnly { get; init; }

    public bool UnconfirmedOnly { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public sealed record UserPage(IReadOnlyList<UserSummary> Items, int TotalCount, int Page, int PageSize);

public sealed record UserSummary(
    string Id,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    bool LockedOut,
    IReadOnlyList<string> Roles);

public sealed record UserDetail(
    string Id,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool PhoneNumberConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    bool TwoFactorEnabled,
    int AccessFailedCount,
    IReadOnlyList<string> Roles,
    IReadOnlyList<UserClaim> Claims,
    IReadOnlyList<UserLogin> Logins);

public sealed record UserClaim(string Type, string Value);

public sealed record UserLogin(string LoginProvider, string ProviderKey, string? ProviderDisplayName);

public sealed record CreateUserRequest(
    string UserName,
    string Email,
    string Password,
    string? PhoneNumber,
    IReadOnlyList<string> Roles);

public sealed record UpdateUserRequest(
    string Id,
    string UserName,
    string Email,
    bool EmailConfirmed,
    string? PhoneNumber,
    bool PhoneNumberConfirmed,
    bool LockoutEnabled,
    DateTimeOffset? LockoutEnd,
    bool TwoFactorEnabled,
    IReadOnlyList<string> Roles,
    IReadOnlyList<UserClaim> Claims);

public sealed record SetPasswordRequest(string Id, string Password);

public sealed record RemoveLoginRequest(string Id, string LoginProvider, string ProviderKey);

public sealed record RoleSummary(string Id, string? Name, IReadOnlyList<UserClaim> Claims);

public sealed record SaveRoleRequest(string? Id, string Name, IReadOnlyList<UserClaim> Claims);

public sealed record ClaimTypeOption(string Name, string Type);