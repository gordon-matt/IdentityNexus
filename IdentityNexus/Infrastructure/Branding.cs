namespace IdentityNexus.Infrastructure;

/// <summary>
/// Header branding. A custom logo wins, then a custom title, then the built-in IdentityNexus logo.
/// </summary>
internal static class Branding
{
    public static string? CustomLogoUrl(IdentityNexusOptions options)
    {
        var path = options.LogoPath?.Trim();

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        // The UI's <base href> is its own mount path, so a relative URL would be requested
        // from there. A leading slash keeps the image on the host's site root.
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

    public static bool HasCustomTitle(IdentityNexusOptions options)
    {
        var title = options.Title?.Trim();

        return !string.IsNullOrEmpty(title)
            && !string.Equals(title, IdentityNexusOptions.DefaultTitle, StringComparison.Ordinal);
    }
}
