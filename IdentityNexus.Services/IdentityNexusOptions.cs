namespace IdentityNexus.Services;

/// <summary>Where the management UI is mounted and how it pages.</summary>
public sealed class IdentityNexusOptions
{
    public const string SectionName = "IdentityNexus";

    public const string DefaultTitle = "IdentityNexus";

    /// <summary>Path the UI is served from. Leading slash, no trailing slash.</summary>
    public string BasePath { get; set; } = "/identity";

    /// <summary>Browser tab title. The header logo and name come from the host's shell branding.</summary>
    public string Title { get; set; } = DefaultTitle;

    /// <summary>Users per page. Clamped to between 1 and 100.</summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// When set, the top bar links here to leave the management UI for the host site.
    /// A root-relative path (<c>/</c>), an app-relative path (<c>~/admin</c>), or an absolute URL.
    /// Leave empty to hide the link. Can be set together with <see cref="LogoutPath"/>.
    /// </summary>
    public string? HomePath { get; set; }

    /// <summary>
    /// When set, the top bar posts here to sign out. Leave empty when the host has no logout endpoint.
    /// </summary>
    public string? LogoutPath { get; set; }

    /// <summary>
    /// Theme area for this document. Null uses the default area, so the UI follows the public site.
    /// Set <c>Admin</c> when IdentityNexus is the back office: it is mounted at <see cref="BasePath"/>,
    /// not under <c>/Admin</c>, and should still share that area's saved theme.
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Which shared menu this document shows. <c>Admin</c> is the usual back office menu.
    /// An empty string is the menu built without a name. Nothing is drawn when that menu was never built.
    /// </summary>
    public string MenuName { get; set; } = "Admin";
}