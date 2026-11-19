using Heroicsoft.NetForge.WebAssets;

namespace IdentityNexus.Infrastructure;

/// <summary>Management UI stylesheet, scripts, and images, served from the assembly at <c>/_content/IdentityNexus/</c>.</summary>
internal static class EmbeddedAssets
{
    /// <summary>Root path of an embedded file, including its content hash.</summary>
    public static string Url(string name) => EmbeddedContent.Url(typeof(EmbeddedAssets).Assembly, name);
}
