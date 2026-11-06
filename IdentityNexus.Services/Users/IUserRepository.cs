namespace IdentityNexus.Services.Users;

/// <summary>
/// Persistence for users. Writes go through <see cref="UserManager{TUser}"/> so passwords,
/// stamps and lockout stay consistent with the host's Identity configuration.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Counts the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (int users, int locked out, int unconfirmed email).</returns>
    Task<(int Users, int LockedOut, int UnconfirmedEmail)> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches the value.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user page.</returns>
    Task<UserPage> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user detail.</returns>
    Task<UserDetail?> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (string? id, identity result result).</returns>
    Task<(string? Id, IdentityResult Result)> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the password.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> SetPasswordAsync(string id, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the login.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="loginProvider">The login provider.</param>
    /// <param name="providerKey">The provider key.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> RemoveLoginAsync(string id, string loginProvider, string providerKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the authenticator.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}