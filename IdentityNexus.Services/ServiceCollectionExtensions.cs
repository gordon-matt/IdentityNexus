using IdentityNexus.Services.Roles;
using IdentityNexus.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityNexus.Services;

public static class ServiceCollectionExtensions
{
    extension<TUser, TRole>(IServiceCollection services)
        where TUser : IdentityUser, new()
        where TRole : IdentityRole, new()
    {
        /// <summary>
        /// Registers the user and role services against the host's existing Identity managers.
        /// The host must already have called <c>AddIdentity</c> (or <c>AddIdentityCore</c> with
        /// roles) for <typeparamref name="TUser"/> and <typeparamref name="TRole"/>.
        /// </summary>
        public IServiceCollection AddIdentityNexusServices(Action<IdentityNexusOptions>? configure = null)
        {
            services.AddOptions<IdentityNexusOptions>()
                .Configure(options => configure?.Invoke(options))
                .PostConfigure(options => options.BasePath = NormalizeBasePath(options.BasePath));

            services.TryAddScoped<AuthenticationStateProvider, AnonymousAuthenticationStateProvider>();
            services.TryAddScoped<IUserRepository, IdentityUserRepository<TUser>>();
            services.TryAddScoped<IRoleRepository, IdentityRoleRepository<TRole>>();
            services.TryAddScoped<IUserService, UserService>();
            services.TryAddScoped<IRoleService, RoleService>();

            return services;
        }
    }

    private static string NormalizeBasePath(string? basePath)
    {
        string trimmed = (basePath ?? string.Empty).Trim().Trim('/');
        return trimmed.Length == 0 ? "/identity" : "/" + trimmed;
    }
}
