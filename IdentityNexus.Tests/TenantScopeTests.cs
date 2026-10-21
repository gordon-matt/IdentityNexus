using Ardalis.Result;
using IdentityNexus;
using IdentityNexus.Services;
using IdentityNexus.Services.Models;
using IdentityNexus.Services.Roles;
using IdentityNexus.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityNexus.Tests;

public class TenantScopeTests
{
    [Fact]
    public async Task UsersAndRoles_AreLimitedToTheCurrentTenant()
    {
        await using var host = await TenantTestHost.CreateAsync();

        host.TenantId = 1;
        var editors = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Editors", []));
        var ada = await host.Users.CreateAsync(new CreateUserRequest("ada", "ada@example.com", "password", null, ["Editors"]));

        host.TenantId = 2;
        var readers = await host.Roles.SaveAsync(new SaveRoleRequest(null, "Readers", []));
        var grace = await host.Users.CreateAsync(new CreateUserRequest("grace", "grace@example.com", "password", null, ["Readers", "Editors"]));

        Assert.Equal(1, (await host.FindUserAsync(ada.Value.Id)).TenantId);
        Assert.Equal(2, (await host.FindUserAsync(grace.Value.Id)).TenantId);
        Assert.Equal(1, (await host.FindRoleAsync(editors.Value.Id)).TenantId);
        Assert.Equal(2, (await host.FindRoleAsync(readers.Value.Id)).TenantId);

        host.TenantId = 1;
        var firstTenantUsers = await host.Users.SearchAsync(new UserListQuery());
        var firstTenantRoles = await host.Roles.ListAsync();
        var firstOverview = await host.Users.GetOverviewAsync();

        Assert.Equal("ada", Assert.Single(firstTenantUsers.Value.Items).UserName);
        Assert.Equal("Editors", Assert.Single(firstTenantRoles.Value).Name);
        Assert.Equal(1, firstOverview.Value.Users);
        Assert.Equal(1, firstOverview.Value.Roles);
        Assert.Equal(ResultStatus.NotFound, (await host.Users.GetAsync(grace.Value.Id)).Status);
        Assert.Equal(ResultStatus.NotFound, (await host.Roles.GetAsync(readers.Value.Id)).Status);
        Assert.Equal(ResultStatus.NotFound, (await host.Users.SetPasswordAsync(new SetPasswordRequest(grace.Value.Id, "other-secret"))).Status);
        Assert.Equal(ResultStatus.NotFound, (await host.Users.DeleteAsync(grace.Value.Id)).Status);
        Assert.Equal(ResultStatus.NotFound, (await host.Roles.DeleteAsync(readers.Value.Id)).Status);
        Assert.Equal(
            ResultStatus.NotFound,
            (await host.Users.UpdateAsync(new UpdateUserRequest(
                grace.Value.Id,
                "grace",
                "grace@example.com",
                EmailConfirmed: true,
                PhoneNumber: null,
                PhoneNumberConfirmed: false,
                LockoutEnabled: false,
                LockoutEnd: null,
                TwoFactorEnabled: false,
                Roles: ["Readers"],
                Claims: []))).Status);

        var byRole = await host.Users.SearchAsync(new UserListQuery { Role = "Editors" });
        Assert.Equal("ada", Assert.Single(byRole.Value.Items).UserName);

        host.TenantId = null;
        var everyone = await host.Users.SearchAsync(new UserListQuery());
        var allRoles = await host.Roles.ListAsync();

        Assert.Equal(2, everyone.Value.TotalCount);
        Assert.Equal(2, allRoles.Value.Count);
        Assert.Equal(ResultStatus.Ok, (await host.Users.GetAsync(grace.Value.Id)).Status);
    }

    [Fact]
    public void Scope_RequiresAWritableProperty()
    {
        var services = new ServiceCollection();

        var error = Assert.Throws<ArgumentException>(() =>
            services.ScopeUsersToTenant<TenantUser, int>(user => user.AccessFailedCount.GetHashCode(), _ => TenantKey<int>.None));

        Assert.Equal("property", error.ParamName);
    }

    private sealed class TenantUser : IdentityUser
    {
        public int? TenantId { get; set; }
    }

    private sealed class TenantRole : IdentityRole
    {
        public int? TenantId { get; set; }
    }

    private sealed class TenantTestHost : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        private readonly ServiceProvider provider;
        private readonly IServiceScope scope;
        private readonly CurrentTenant current;

        private TenantTestHost(SqliteConnection connection, ServiceProvider provider, IServiceScope scope, CurrentTenant current)
        {
            this.connection = connection;
            this.provider = provider;
            this.scope = scope;
            this.current = current;
            Users = scope.ServiceProvider.GetRequiredService<IUserService>();
            Roles = scope.ServiceProvider.GetRequiredService<IRoleService>();
        }

        public IUserService Users { get; }

        public IRoleService Roles { get; }

        public int? TenantId
        {
            get => current.Id;
            set => current.Id = value;
        }

        public static async Task<TenantTestHost> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var current = new CurrentTenant();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(current);
            services.AddDbContext<TenantTestDbContext>(options => options.UseSqlite(connection));
            services
                .AddIdentity<TenantUser, TenantRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<TenantTestDbContext>()
                .AddDefaultTokenProviders();

            services.AddIdentityNexusServices<TenantUser, TenantRole>();
            services.ScopeUsersToTenant<TenantUser, int?>(user => user.TenantId, _ => TenantKey.OfNullable(current.Id));
            services.ScopeRolesToTenant<TenantRole, int?>(role => role.TenantId, _ => TenantKey.OfNullable(current.Id));

            var provider = services.BuildServiceProvider();
            var scope = provider.CreateScope();
            await scope.ServiceProvider.GetRequiredService<TenantTestDbContext>().Database.EnsureCreatedAsync();

            return new TenantTestHost(connection, provider, scope, current);
        }

        public async Task<TenantUser> FindUserAsync(string id) =>
            (await scope.ServiceProvider.GetRequiredService<UserManager<TenantUser>>().FindByIdAsync(id))!;

        public async Task<TenantRole> FindRoleAsync(string id) =>
            (await scope.ServiceProvider.GetRequiredService<RoleManager<TenantRole>>().FindByIdAsync(id))!;

        public async ValueTask DisposeAsync()
        {
            scope.Dispose();
            await provider.DisposeAsync();
            await connection.DisposeAsync();
        }

        private sealed class CurrentTenant
        {
            public int? Id { get; set; }
        }

        private sealed class TenantTestDbContext(DbContextOptions<TenantTestDbContext> options)
            : IdentityDbContext<TenantUser, TenantRole, string>(options);
    }
}
