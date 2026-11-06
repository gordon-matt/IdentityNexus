namespace IdentityNexus.Services.Models;

/// <summary>
/// Represents the identity overview.
/// </summary>
/// <param name="Users">The users.</param>
/// <param name="Roles">The roles.</param>
/// <param name="LockedOut">The locked out.</param>
/// <param name="UnconfirmedEmail">The unconfirmed email.</param>
public sealed record IdentityOverview(int Users, int Roles, int LockedOut, int UnconfirmedEmail);

/// <summary>
/// Represents the user list query.
/// </summary>
public sealed record UserListQuery
{
    /// <summary>
    /// Gets or sets the search.
    /// </summary>
    public string? Search { get; init; }

    /// <summary>
    /// Gets or sets the role.
    /// </summary>
    public string? Role { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is locked out only.
    /// </summary>
    public bool LockedOutOnly { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is unconfirmed only.
    /// </summary>
    public bool UnconfirmedOnly { get; init; }

    /// <summary>
    /// Gets or sets the page.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; init; } = 25;
}

/// <summary>
/// Represents the user page.
/// </summary>
/// <param name="Items">The items.</param>
/// <param name="TotalCount">The total count.</param>
/// <param name="Page">The page.</param>
/// <param name="PageSize">The page size.</param>
public sealed record UserPage(IReadOnlyList<UserSummary> Items, int TotalCount, int Page, int PageSize);

/// <summary>
/// Represents the user summary.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email.</param>
/// <param name="EmailConfirmed">The email confirmed.</param>
/// <param name="LockedOut">The locked out.</param>
/// <param name="Roles">The roles.</param>
public sealed record UserSummary(
    string Id,
    string? UserName,
    string? Email,
    bool EmailConfirmed,
    bool LockedOut,
    IReadOnlyList<string> Roles);

/// <summary>
/// Represents the user detail.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email.</param>
/// <param name="EmailConfirmed">The email confirmed.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="PhoneNumberConfirmed">The phone number confirmed.</param>
/// <param name="LockoutEnabled">The lockout enabled.</param>
/// <param name="LockoutEnd">The lockout end.</param>
/// <param name="TwoFactorEnabled">The two factor enabled.</param>
/// <param name="AccessFailedCount">The access failed count.</param>
/// <param name="Roles">The roles.</param>
/// <param name="Claims">The claims.</param>
/// <param name="Logins">The logins.</param>
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

/// <summary>
/// Represents the user claim.
/// </summary>
/// <param name="Type">The type.</param>
/// <param name="Value">The value.</param>
public sealed record UserClaim(string Type, string Value);

/// <summary>
/// Represents the user login.
/// </summary>
/// <param name="LoginProvider">The login provider.</param>
/// <param name="ProviderKey">The provider key.</param>
/// <param name="ProviderDisplayName">The provider display name.</param>
public sealed record UserLogin(string LoginProvider, string ProviderKey, string? ProviderDisplayName);

/// <summary>
/// Represents the create user request.
/// </summary>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email.</param>
/// <param name="Password">The password.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="Roles">The roles.</param>
public sealed record CreateUserRequest(
    string UserName,
    string Email,
    string Password,
    string? PhoneNumber,
    IReadOnlyList<string> Roles);

/// <summary>
/// Represents the update user request.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="UserName">The user name.</param>
/// <param name="Email">The email.</param>
/// <param name="EmailConfirmed">The email confirmed.</param>
/// <param name="PhoneNumber">The phone number.</param>
/// <param name="PhoneNumberConfirmed">The phone number confirmed.</param>
/// <param name="LockoutEnabled">The lockout enabled.</param>
/// <param name="LockoutEnd">The lockout end.</param>
/// <param name="TwoFactorEnabled">The two factor enabled.</param>
/// <param name="Roles">The roles.</param>
/// <param name="Claims">The claims.</param>
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

/// <summary>
/// Represents the set password request.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Password">The password.</param>
public sealed record SetPasswordRequest(string Id, string Password);

/// <summary>
/// Represents the remove login request.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="LoginProvider">The login provider.</param>
/// <param name="ProviderKey">The provider key.</param>
public sealed record RemoveLoginRequest(string Id, string LoginProvider, string ProviderKey);

/// <summary>
/// Represents the role summary.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Claims">The claims.</param>
public sealed record RoleSummary(string Id, string? Name, IReadOnlyList<UserClaim> Claims);

/// <summary>
/// Represents the save role request.
/// </summary>
/// <param name="Id">The identifier.</param>
/// <param name="Name">The name.</param>
/// <param name="Claims">The claims.</param>
public sealed record SaveRoleRequest(string? Id, string Name, IReadOnlyList<UserClaim> Claims);

/// <summary>
/// Represents the claim type option.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Type">The type.</param>
public sealed record ClaimTypeOption(string Name, string Type);