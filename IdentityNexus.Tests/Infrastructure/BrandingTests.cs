using IdentityNexus.Infrastructure;
using IdentityNexus.Services;

namespace IdentityNexus.Tests.Infrastructure;

public class BrandingTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("/brand/logo.png", "/brand/logo.png")]
    [InlineData("~/brand/logo.png", "/brand/logo.png")]
    [InlineData("brand/logo.png", "/brand/logo.png")]
    [InlineData("https://cdn.example.com/logo.png", "https://cdn.example.com/logo.png")]
    public void CustomLogoUrl_NormalizesThePath(string? path, string? expected)
    {
        var url = Branding.CustomLogoUrl(new IdentityNexusOptions { LogoPath = path });

        Assert.Equal(expected, url);
    }

    [Theory]
    [InlineData("IdentityNexus", false)]
    [InlineData(" IdentityNexus ", false)]
    [InlineData("Acme Identity", true)]
    [InlineData("", false)]
    public void HasCustomTitle_IsAnythingOtherThanTheDefault(string title, bool expected)
    {
        Assert.Equal(expected, Branding.HasCustomTitle(new IdentityNexusOptions { Title = title }));
    }
}