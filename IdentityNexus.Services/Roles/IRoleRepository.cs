namespace IdentityNexus.Services.Roles;

/// <summary>
/// Defines the contract for the role repository.
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Counts the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the int.</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role summaries.</returns>
    Task<IReadOnlyList<RoleSummary>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the role summary.</returns>
    Task<RoleSummary?> FindAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates the value.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="claims">The claims.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the (string? id, identity result result).</returns>
    Task<(string? Id, IdentityResult Result)> CreateAsync(string name, IReadOnlyList<UserClaim> claims, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="claims">The claims.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> UpdateAsync(string id, string name, IReadOnlyList<UserClaim> claims, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identity result.</returns>
    Task<IdentityResult> DeleteAsync(string id, CancellationToken cancellationToken = default);
}