namespace IdentityNexus.Services;

/// <summary>Where the management UI is mounted and how it pages.</summary>
public sealed class IdentityNexusOptions
{
    public const string SectionName = "IdentityNexus";

    public const string DefaultTitle = "IdentityNexus";

    /// <summary>Path the UI is served from. Leading slash, no trailing slash.</summary>
    public string BasePath { get; set; } = "/identity";

    /// <summary>
    /// Browser tab title. Also shown in the header when <see cref="LogoPath"/> is unset and this
    /// is not <see cref="DefaultTitle"/>.
    /// </summary>
    public string Title { get; set; } = DefaultTitle;

    /// <summary>
    /// Header image. Takes precedence over <see cref="Title"/>; otherwise a custom title is shown,
    /// and the IdentityNexus logo is used for the default title. A root-relative path (<c>/logo.png</c>),
    /// an app-relative path (<c>~/logo.png</c>), or an absolute URL.
    /// </summary>
    public string? LogoPath { get; set; }

    /// <summary>Users per page. Clamped to between 1 and 100.</summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// When set, the top bar posts here to sign out. Leave empty when the host has no logout endpoint.
    /// </summary>
    public string? LogoutPath { get; set; }
}
