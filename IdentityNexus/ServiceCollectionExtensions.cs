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
            services.AddIdentityNexusServices<TUser, TRole>(configure);
            return services;
        }
    }
}
