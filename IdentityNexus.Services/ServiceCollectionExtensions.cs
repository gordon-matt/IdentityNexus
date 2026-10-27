using IdentityNexus.Services.Roles;
using IdentityNexus.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;

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
                .PostConfigure(options =>
                {
                    options.BasePath = NormalizeBasePath(options.BasePath);
                    options.HomePath = NormalizeHomePath(options.HomePath);
                });

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

    private static string? NormalizeHomePath(string? path)
    {
        path = path?.Trim();
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        // The UI's <base href> is its mount path, so a relative URL would stay inside IdentityNexus.
        if (path.StartsWith("~/", StringComparison.Ordinal))
        {
            path = path[1..];
        }

        if (path.StartsWith('/') || Uri.TryCreate(path, UriKind.Absolute, out _))
        {
            return path;
        }

        return "/" + path;
    }
}