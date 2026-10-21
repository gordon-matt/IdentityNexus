using System.Linq.Expressions;
using Heroicsoft.NetForge.Abstractions.Auth;
using Heroicsoft.NetForge.Components;
using IdentityNexus.Services.Tenancy;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityNexus;

public static class ServiceCollectionExtensions
{
    extension<TUser, TRole>(IServiceCollection services)
        where TUser : IdentityUser, new()
        where TRole : IdentityRole, new()
    {
        /// <summary>
        /// Registers the management UI and its services. The host must already have registered
        /// Identity for <typeparamref name="TUser"/> and <typeparamref name="TRole"/>, including
        /// an Entity Framework store, then call <c>app.MapIdentityNexus()</c>.
        /// </summary>
        public IServiceCollection AddIdentityNexus(Action<IdentityNexusOptions>? configure = null)
        {
            // Registered first so the circuit's authentication state wins over the anonymous fallback.
            services.AddRazorComponents().AddInteractiveServerComponents();
            // The shell reads ForgeUiOptions for the shared theme key. A host that also calls
            // AddNetForgeComponents keeps its own key; this only fills the options in when it does not.
            services.AddNetForgeComponents();
            services.TryAddSingleton<IAuthProviderService>(new AuthProviderService(AuthProvider.Identity, null));
            services.AddIdentityNexusServices<TUser, TRole>(configure);
            return services;
        }
    }

    /// <summary>
    /// Limits user queries to the current tenant and stamps that id onto users IdentityNexus creates.
    /// <see cref="TenantKey{TKey}.None"/> leaves the users unscoped.
    /// </summary>
    public static IServiceCollection ScopeUsersToTenant<TUser, TKey>(
        this IServiceCollection services,
        Expression<Func<TUser, TKey>> tenantId,
        Func<IServiceProvider, TenantKey<TKey>> currentTenantId)
        where TUser : class
    {
        services.AddUser(tenantId, services => ToScope(currentTenantId(services)));
        return services;
    }

    /// <summary>
    /// Limits role queries to the current tenant and stamps that id onto roles IdentityNexus creates.
    /// <see cref="TenantKey{TKey}.None"/> leaves the roles unscoped.
    /// </summary>
    public static IServiceCollection ScopeRolesToTenant<TRole, TKey>(
        this IServiceCollection services,
        Expression<Func<TRole, TKey>> tenantId,
        Func<IServiceProvider, TenantKey<TKey>> currentTenantId)
        where TRole : class
    {
        services.AddRole(tenantId, services => ToScope(currentTenantId(services)));
        return services;
    }

    private static TenantScopeValue<TKey> ToScope<TKey>(TenantKey<TKey> key) =>
        new(key.HasValue, key.Value);
}
