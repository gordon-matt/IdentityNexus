using Heroicsoft.NetForge.Components.Navigation;

namespace IdentityNexus;

/// <summary>Puts the management pages on the host menu. The host places them with <c>AddIdentityNexus()</c>.</summary>
public sealed class IdentityNexusMenuContributor(IOptions<IdentityNexusOptions> options) : INetForgeMenuContributor
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public string Name => "IdentityNexus";

    /// <summary>
    /// Gets the default menu.
    /// </summary>
    public string DefaultMenu => "Membership";

    /// <summary>
    /// Performs the contribute operation.
    /// </summary>
    /// <param name="group">The group.</param>
    /// <param name="configure">The configure.</param>
    public void Contribute(INetForgeMenuGroupBuilder group, Action<NetForgeMenuItemOptions>? configure)
    {
        string root = options.Value.BasePath.TrimEnd('/');
        group.AddSubmenu(
            "Identity",
            children =>
            {
                children.AddCustom(item =>
                {
                    item.Title = "Overview";
                    item.Icon = "layout";
                    item.Url = root;
                    item.MatchAll = true;
                });
                children.AddCustom(item =>
                {
                    item.Title = "Users";
                    item.Icon = "users";
                    item.Url = root + "/users";
                });
                children.AddCustom(item =>
                {
                    item.Title = "Roles";
                    item.Icon = "shield";
                    item.Url = root + "/roles";
                });
            },
            item =>
            {
                item.Icon = "users";
                configure?.Invoke(item);
            });
    }
}

/// <summary>Places the IdentityNexus contributor. The host can retitle the parent; the child pages stay.</summary>
public static class IdentityNexusMenuExtensions
{
    extension(INetForgeMenuGroupBuilder menu)
    {
        /// <summary>
        /// Adds the identity nexus.
        /// </summary>
        /// <param name="configure">The configure.</param>
        /// <returns>The identity nexus.</returns>
        public INetForgeMenuGroupBuilder AddIdentityNexus(Action<NetForgeMenuItemOptions>? configure = null) =>
            menu.AddContributor("IdentityNexus", configure);
    }
}