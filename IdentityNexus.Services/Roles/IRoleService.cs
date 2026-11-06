namespace IdentityNexus.Services.Roles;

/// <summary>
/// Defines the contract for the role service.
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Lists the value.
    /// </summary>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the role summaries.</returns>
    Task<Result<IReadOnlyList<RoleSummary>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the role summary.</returns>
    Task<Result<RoleSummary>> GetAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves the value.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is the outcome for the role summary.</returns>
    Task<Result<RoleSummary>> SaveAsync(SaveRoleRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the value.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result.</returns>
    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}