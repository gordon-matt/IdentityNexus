using Ardalis.Result;
using IdentityNexus.Services;
using IdentityNexus.Services.Models;

namespace IdentityNexus.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Create_ThenSearch_FindsTheUser()
    {
        using var host = await IdentityTestHost.CreateAsync();

        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));
        var page = await host.Users.SearchAsync(new UserListQuery { Search = "ADA" });

        Assert.Equal(ResultStatus.Created, created.Status);
        Assert.Equal("ada", page.Value.Items.Single().UserName);
        Assert.Equal(1, page.Value.TotalCount);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Conflicts()
    {
        using var host = await IdentityTestHost.CreateAsync();
        await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));

        var duplicate = await host.Users.CreateAsync(new CreateUserRequest("ada2", "ada@example.com", "password", null, []));

        Assert.Equal(ResultStatus.Conflict, duplicate.Status);
    }

    [Fact]
    public async Task Update_PersistsProfileRolesAndClaims()
    {
        using var host = await IdentityTestHost.CreateAsync();
        await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));

        var updated = await host.Users.UpdateAsync(new UpdateUserRequest(
            created.Value.Id,
            "ada.lovelace",
            "ada@example.com",
            EmailConfirmed: true,
            PhoneNumber: "555",
            PhoneNumberConfirmed: true,
            LockoutEnabled: true,
            LockoutEnd: DateTimeOffset.UtcNow.AddHours(2),
            TwoFactorEnabled: false,
            Roles: ["Editors"],
            Claims: [new UserClaim("department", "analysis")]));

        Assert.True(updated.IsSuccessStatus());
        Assert.Equal("ada.lovelace", updated.Value.UserName);
        Assert.Equal("555", updated.Value.PhoneNumber);
        Assert.True(updated.Value.PhoneNumberConfirmed);
        Assert.Equal("Editors", Assert.Single(updated.Value.Roles));
        Assert.Contains(updated.Value.Claims, claim => claim.Type == "department" && claim.Value == "analysis");
        Assert.True(updated.Value.LockoutEnd > DateTimeOffset.UtcNow);

        var locked = await host.Users.SearchAsync(new UserListQuery { LockedOutOnly = true });
        Assert.True(locked.IsSuccessStatus(), string.Join(" | ", locked.GetDisplayMessages()));
        Assert.Equal(created.Value.Id, Assert.Single(locked.Value.Items).Id);
    }

    [Fact]
    public async Task SetPassword_ReplacesTheExistingPassword()
    {
        using var host = await IdentityTestHost.CreateAsync();
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));

        var result = await host.Users.SetPasswordAsync(new SetPasswordRequest(created.Value.Id, "new-secret"));
        var user = await host.UserManager.FindByIdAsync(created.Value.Id);

        Assert.True(result.IsSuccessStatus());
        Assert.True(await host.UserManager.CheckPasswordAsync(user!, "new-secret"));
        Assert.False(await host.UserManager.CheckPasswordAsync(user!, "password"));
    }

    [Fact]
    public async Task Delete_SignedInAccount_Conflicts()
    {
        using var host = await IdentityTestHost.CreateAsync();
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));
        host.SignInAs(created.Value.Id);

        var result = await host.Users.DeleteAsync(created.Value.Id);
        var stillThere = await host.Users.GetAsync(created.Value.Id);

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.True(stillThere.IsSuccessStatus());
    }

    [Fact]
    public async Task Delete_RemovesTheUser()
    {
        using var host = await IdentityTestHost.CreateAsync();
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));

        var deleted = await host.Users.DeleteAsync(created.Value.Id);
        var missing = await host.Users.GetAsync(created.Value.Id);

        Assert.True(deleted.IsSuccessStatus());
        Assert.Equal(ResultStatus.NotFound, missing.Status);
    }

    [Fact]
    public async Task GetOverview_CountsUsersRolesAndLockouts()
    {
        using var host = await IdentityTestHost.CreateAsync();
        await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));
        await host.Users.UpdateAsync(new UpdateUserRequest(
            created.Value.Id,
            "ada",
            "ada@example.com",
            false,
            null,
            false,
            true,
            DateTimeOffset.UtcNow.AddDays(1),
            false,
            [],
            []));

        var overview = await host.Users.GetOverviewAsync();

        Assert.Equal(1, overview.Value.Users);
        Assert.Equal(1, overview.Value.Roles);
        Assert.Equal(1, overview.Value.LockedOut);
        Assert.Equal(1, overview.Value.UnconfirmedEmail);
    }
}
