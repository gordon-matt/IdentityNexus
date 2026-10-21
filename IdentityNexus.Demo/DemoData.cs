namespace IdentityNexus.Demo;

public static class DemoData
{
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "Password1!";
    public const string AdminRole = "Administrators";

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var database = provider.GetRequiredService<ApplicationDbContext>();
        await database.Database.EnsureCreatedAsync();

        var roles = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        if (!await roles.RoleExistsAsync(AdminRole))
        {
            var created = await roles.CreateAsync(new ApplicationRole { Name = AdminRole });
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(created.Errors.First().Description);
            }
        }

        var users = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await users.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
            };

            var created = await users.CreateAsync(admin, AdminPassword);
            if (!created.Succeeded)
            {
                throw new InvalidOperationException(created.Errors.First().Description);
            }
        }

        if (!await users.IsInRoleAsync(admin, AdminRole))
        {
            await users.AddToRoleAsync(admin, AdminRole);
        }
    }
}
