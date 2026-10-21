using Ardalis.Result;
using IdentityNexus.Services;
using IdentityNexus.Services.Models;

namespace IdentityNexus.Tests;

public class RoleServiceTests
{
    [Fact]
    public async Task Save_CreateRenameAndClaims_RoundTrips()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var created = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", [new UserClaim("department", "news")]));
        var renamed = await host.Roles.SaveAsync(new SaveRoleRequest(created.Value.Id, "Senior editors", [new UserClaim("department", "news"), new UserClaim("desk", "city")]));

        Assert.Equal(ResultStatus.Created, created.Status);
        Assert.Equal("Senior editors", renamed.Value.Name);
        Assert.Equal(2, renamed.Value.Claims.Count);
    }

    [Fact]
    public async Task Save_BlankName_IsInvalid()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var result = await host.Roles.SaveAsync(new SaveRoleRequest(null, "  ", []));

        Assert.Equal(ResultStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task Delete_RemovesTheRole()
    {
        using var host = await IdentityTestHost.CreateAsync();
        var created = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));

        var deleted = await host.Roles.DeleteAsync(created.Value.Id);
        var missing = await host.Roles.GetAsync(created.Value.Id);

        Assert.True(deleted.IsSuccessStatus());
        Assert.Equal(ResultStatus.NotFound, missing.Status);
    }
}
