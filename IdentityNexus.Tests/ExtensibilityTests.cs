using System.Security.Claims;
using Ardalis.Result;
using IdentityNexus.Extensibility;
using IdentityNexus.Services;
using IdentityNexus.Services.Extensibility;
using IdentityNexus.Services.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityNexus.Tests;

public class ExtensibilityTests
{
    private const string Managed = "permission";

    [Fact]
    public async Task UserUpdate_ManagedClaimType_IsHiddenAndKept()
    {
        using var host = await IdentityTestHost.CreateAsync(options => options.ManagedClaimTypes.Add(Managed));
        var created = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));
        var user = (await host.UserManager.FindByIdAsync(created.Value.Id))!;
        await host.UserManager.AddClaimAsync(user, new Claim(Managed, "pages.write"));

        var updated = await host.Users.UpdateAsync(new UpdateUserRequest(
            created.Value.Id, "ada", "ada@example.com", true, null, false, true, null, false, [],
            [new UserClaim("department", "analysis"), new UserClaim(Managed, "everything")]));

        var stored = await host.UserManager.GetClaimsAsync(user);
        Assert.True(updated.IsSuccessStatus());
        Assert.DoesNotContain(updated.Value.Claims, claim => claim.Type == Managed);
        Assert.Contains(stored, claim => claim.Type == Managed && claim.Value == "pages.write");
        Assert.DoesNotContain(stored, claim => claim.Value == "everything");
        Assert.Contains(stored, claim => claim.Type == "department");
    }

    [Fact]
    public async Task RoleSave_ManagedClaimType_IsHiddenAndKept()
    {
        using var host = await IdentityTestHost.CreateAsync(options => options.ManagedClaimTypes.Add(Managed));
        var created = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));
        var role = (await host.RoleManager.FindByIdAsync(created.Value.Id))!;
        await host.RoleManager.AddClaimAsync(role, new Claim(Managed, "pages.write"));

        var saved = await host.Roles.SaveAsync(new SaveRoleRequest(created.Value.Id, "Editors", [new UserClaim("desk", "city")]));

        var stored = await host.RoleManager.GetClaimsAsync(role);
        Assert.Equal("desk", Assert.Single(saved.Value.Claims).Type);
        Assert.Contains(stored, claim => claim.Type == Managed && claim.Value == "pages.write");
    }

    [Fact]
    public async Task Writes_NotifyObservers()
    {
        var observer = new RecordingObserver();
        using var host = await IdentityTestHost.CreateAsync(extraServices: services =>
        {
            services.AddSingleton(observer);
            services.AddIdentityNexusObserver<ForwardingObserver>();
        });

        var role = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));
        var user = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, []));
        await host.Roles.DeleteAsync(role.Value.Id);

        Assert.Equal(
            [
                new IdentityChange(IdentitySubjectKind.Role, role.Value.Id, IdentityChangeType.Created),
                new IdentityChange(IdentitySubjectKind.User, user.Value.Id, IdentityChangeType.Created),
                new IdentityChange(IdentitySubjectKind.Role, role.Value.Id, IdentityChangeType.Deleted),
            ],
            observer.Changes);
    }

    [Fact]
    public async Task Observer_Failure_DoesNotFailTheWrite()
    {
        using var host = await IdentityTestHost.CreateAsync(extraServices: services =>
            services.AddIdentityNexusObserver<ThrowingObserver>());

        var role = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));

        Assert.Equal(ResultStatus.Created, role.Status);
    }

    [Fact]
    public void AddUserSection_AddsTheTabAndManagesItsClaimTypes()
    {
        var options = new IdentityNexusOptions();

        options.AddUserSection<NoopSection>("Permissions", Managed);

        var section = Assert.Single(options.UserSections);
        Assert.Equal("Permissions", section.Title);
        Assert.Equal(typeof(NoopSection), section.ComponentType);
        Assert.Contains(Managed, options.ManagedClaimTypes);
        Assert.Empty(options.RoleSections);
    }

    [Fact]
    public void AddRoleSection_BlankTitle_Throws()
    {
        var options = new IdentityNexusOptions();

        Assert.Throws<ArgumentException>(() => options.AddRoleSection<NoopSection>(" "));
    }

    public sealed class RecordingObserver
    {
        public List<IdentityChange> Changes { get; } = [];
    }

    private sealed class ForwardingObserver(RecordingObserver recorder) : IIdentityChangeObserver
    {
        public Task OnChangedAsync(IdentityChange change, CancellationToken cancellationToken = default)
        {
            recorder.Changes.Add(change);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingObserver : IIdentityChangeObserver
    {
        public Task OnChangedAsync(IdentityChange change, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Observer failed.");
    }

    private sealed class NoopSection : IdentityNexusSection
    {
        public override Task<Result> SaveAsync(string id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success());
    }
}