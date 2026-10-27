namespace IdentityNexus.Demo;

public sealed class ApplicationUser : IdentityUser
{
}

public sealed class ApplicationRole : IdentityRole
{
}

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options);