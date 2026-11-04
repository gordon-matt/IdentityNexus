using System.Security.Claims;
using IdentityNexus.Services.Claims;

namespace IdentityNexus.Tests;

public class ClaimTypeCatalogTests
{
    [Fact]
    public void All_WellKnownClaimTypes_ListsThemByName()
    {
        Assert.Contains(ClaimTypeCatalog.All, option => option.Name == nameof(ClaimTypes.Email) && option.Type == ClaimTypes.Email);
        Assert.Equal(nameof(ClaimTypes.Email), ClaimTypeCatalog.DisplayName(ClaimTypes.Email));
    }

    [Fact]
    public void DisplayName_CustomType_ReturnsTheType()
    {
        Assert.Equal("department", ClaimTypeCatalog.DisplayName("department"));
    }
}