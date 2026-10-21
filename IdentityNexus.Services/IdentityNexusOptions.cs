namespace IdentityNexus.Services;

/// <summary>Where the management UI is mounted and how it pages.</summary>
public sealed class IdentityNexusOptions
{
    public const string SectionName = "IdentityNexus";

    /// <summary>Path the UI is served from. Leading slash, no trailing slash.</summary>
    public string BasePath { get; set; } = "/identity";

    /// <summary>Shown in the browser title and the top bar.</summary>
    public string Title { get; set; } = "IdentityNexus";

    /// <summary>Users per page. Clamped to between 1 and 100.</summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// When set, the top bar posts here to sign out. Leave empty when the host has no logout endpoint.
    /// </summary>
    public string? LogoutPath { get; set; }
}
