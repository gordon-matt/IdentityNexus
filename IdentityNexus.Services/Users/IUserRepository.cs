namespace IdentityNexus.Services.Users;

/// <summary>
/// Persistence for users. Writes go through <see cref="UserManager{TUser}"/> so passwords,
/// stamps and lockout stay consistent with the host's Identity configuration.
/// </summary>
public interface IUserRepository
{
    Task<(int Users, int LockedOut, int UnconfirmedEmail)> CountAsync(CancellationToken cancellationToken = default);

    Task<UserPage> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default);

    Task<UserDetail?> FindAsync(string id, CancellationToken cancellationToken = default);

    Task<(string? Id, IdentityResult Result)> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<IdentityResult> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<IdentityResult> SetPasswordAsync(string id, string password, CancellationToken cancellationToken = default);

    Task<IdentityResult> RemoveLoginAsync(string id, string loginProvider, string providerKey, CancellationToken cancellationToken = default);

    Task<IdentityResult> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default);

    Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
