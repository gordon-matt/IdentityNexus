using System.Security.Claims;
using IdentityNexus.Services;
using IdentityNexus.Services.Roles;
using IdentityNexus.Services.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdentityNexus.Tests;

public sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection connection;
    private readonly ServiceProvider provider;
    private readonly IServiceScope scope;

    private IdentityTestHost(SqliteConnection connection, ServiceProvider provider, IServiceScope scope, TestAuth auth)
    {
        this.connection = connection;
        this.provider = provider;
        this.scope = scope;
        Auth = auth;
        Users = scope.ServiceProvider.GetRequiredService<IUserService>();
        Roles = scope.ServiceProvider.GetRequiredService<IRoleService>();
        UserManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    }

    public TestAuth Auth { get; }

    public IUserService Users { get; }

    public IRoleService Roles { get; }

    public UserManager<IdentityUser> UserManager { get; }

    public static async Task<IdentityTestHost> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var auth = new TestAuth();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton(auth);
        services.AddSingleton<AuthenticationStateProvider>(auth);
        services.AddDbContext<IdentityTestDbContext>(options => options.UseSqlite(connection));
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<IdentityTestDbContext>()
            .AddDefaultTokenProviders();

        services.AddIdentityNexusServices<IdentityUser, IdentityRole>();

        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IdentityTestDbContext>();
        await database.Database.EnsureCreatedAsync();

        return new IdentityTestHost(connection, provider, scope, auth);
    }

    public void SignInAs(string userId) =>
        Auth.Principal = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)],
            authenticationType: "test"));

    public void Dispose()
    {
        scope.Dispose();
        provider.Dispose();
        connection.Dispose();
    }

    private sealed class IdentityTestDbContext(DbContextOptions<IdentityTestDbContext> options)
        : IdentityDbContext<IdentityUser, IdentityRole, string>(options);
}

public sealed class TestAuth : AuthenticationStateProvider
{
    public ClaimsPrincipal Principal { get; set; } = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(Principal));
}
