namespace IdentityNexus.Services.Users;

public interface IUserService
{
    Task<Result<IdentityOverview>> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<Result<UserPage>> SearchAsync(UserListQuery query, CancellationToken cancellationToken = default);

    Task<Result<UserDetail>> GetAsync(string id, CancellationToken cancellationToken = default);

    Task<Result<UserDetail>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result<UserDetail>> UpdateAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task<Result> SetPasswordAsync(SetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<Result> RemoveLoginAsync(RemoveLoginRequest request, CancellationToken cancellationToken = default);

    Task<Result> ResetAuthenticatorAsync(string id, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(string id, CancellationToken cancellationToken = default);
}