using IdentityNexus.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace IdentityNexus.Tests;

public class IdentityNexusOptionsTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("/", "/")]
    [InlineData("/admin", "/admin")]
    [InlineData("~/admin", "/admin")]
    [InlineData("admin", "/admin")]
    [InlineData("https://example.com/home", "https://example.com/home")]
    public void HomePath_IsRootedSoTheBaseHrefDoesNotCaptureIt(string? path, string? expected)
    {
        var services = new ServiceCollection();
        services.AddIdentityNexusServices<IdentityUser, IdentityRole>(options => options.HomePath = path);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<IdentityNexusOptions>>().Value;

        Assert.Equal(expected, options.HomePath);
    }
}
