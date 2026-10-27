using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace IdentityNexus.Infrastructure;

internal static class IdentityNexusRenderMode
{
    public static IComponentRenderMode Interactive { get; } = new InteractiveServerRenderMode(prerender: false);
}