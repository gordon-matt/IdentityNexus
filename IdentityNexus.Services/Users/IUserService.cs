namespace IdentityNexus.Services.Users;

/// <summary>
/// Defines the contract for the user service.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the overview.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the identity overview.</returns>
    Task<Result<IdentityOverview>> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches the value.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the user page.</returns>
    Task<Result<UserPage>> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the user detail.</returns>
    Task<Result<UserDetail>> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the user detail.</returns>
    Task<Result<UserDetail>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the user detail.</returns>
    Task<Result<UserDetail>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the password.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the login.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> RemoveLoginAsync(RemoveLoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the authenticator.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}