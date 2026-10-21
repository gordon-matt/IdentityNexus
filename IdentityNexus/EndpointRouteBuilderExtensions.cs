using IdentityNexus.Components;
using IdentityNexus.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;

namespace IdentityNexus;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder endpoints)
    {
        /// <summary>
        /// Mounts the management UI at <see cref="IdentityNexusOptions.BasePath"/>. Secure the
        /// returned group the same way as any other endpoint:
        /// <code>app.MapIdentityNexus().RequireAuthorization("Administrators");</code>
        /// </summary>
        public RouteGroupBuilder MapIdentityNexus()
        {
            var options = endpoints.ServiceProvider.GetRequiredService<IOptions<IdentityNexusOptions>>().Value;
            var group = endpoints.MapGroup(options.BasePath);

            group.MapGet("_assets/{name}", EmbeddedAssets.Serve).ExcludeFromDescription();
            group.MapGet("_framework/blazor.web.js", ServeBlazorScript).ExcludeFromDescription();

            group.MapRazorComponents<IdentityNexusApp>()
                .AddInteractiveServerRenderMode()
                .DisableAntiforgery();

            return group;
        }
    }

    private static IResult ServeBlazorScript(IWebHostEnvironment environment)
    {
        var file = environment.WebRootFileProvider.GetFileInfo("_framework/blazor.web.js");

        return !file.Exists
            ? Results.Problem(
                "blazor.web.js was not found in the web root. IdentityNexus needs the host to be built with " +
                "the ASP.NET Core Web SDK, and static web assets to be enabled outside Development " +
                "(they are copied into wwwroot on publish).",
                statusCode: StatusCodes.Status404NotFound)
            : file.PhysicalPath is { } physicalPath
            ? Results.File(physicalPath, "text/javascript")
            : Results.Stream(file.CreateReadStream(), "text/javascript", lastModified: file.LastModified);
    }
}
