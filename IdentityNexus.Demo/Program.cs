using IdentityNexus.Demo;
using IdentityNexus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

string provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
string? connectionString = builder.Configuration.GetConnectionString("IdentityNexus");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    switch (provider)
    {
        case "Sqlite":
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                string path = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "identity.db");
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                connectionString = $"Data Source={path}";
            }

            options.UseSqlite(connectionString);
            break;

        case "SqlServer":
            options.UseSqlServer(RequiredConnection(provider, connectionString));
            break;

        case "Npgsql":
            options.UseNpgsql(RequiredConnection(provider, connectionString));
            break;

        case "MySql":
            options.UseMySQL(RequiredConnection(provider, connectionString));
            break;

        default:
            throw new InvalidOperationException(
                $"Unknown Database:Provider '{provider}'. Expected Sqlite, SqlServer, Npgsql or MySql.");
    }
});

builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/login";
});

builder.Services.AddIdentityNexus<ApplicationUser, ApplicationRole>(options =>
{
    options.LogoutPath = "/account/logout";
});

var app = builder.Build();

await DemoData.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/identity"));

app.MapGet("/account/login", (string? returnUrl) =>
    Results.Content(LoginPage.Html(LocalReturn(returnUrl)), "text/html"));

app.MapPost("/account/login", async (HttpRequest request, UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) =>
{
    var form = await request.ReadFormAsync();
    string email = form["email"].ToString();
    string password = form["password"].ToString();
    string returnUrl = LocalReturn(form["returnUrl"].ToString());

    var user = await users.FindByEmailAsync(email);
    if (user is null)
    {
        return Results.Content(LoginPage.Html(returnUrl, "Those credentials were not accepted."), "text/html");
    }

    var result = await signIn.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
    return result.Succeeded
        ? Results.Redirect(returnUrl)
        : Results.Content(LoginPage.Html(returnUrl, "Those credentials were not accepted."), "text/html");
}).DisableAntiforgery();

app.MapPost("/account/logout", async (SignInManager<ApplicationUser> signIn) =>
{
    await signIn.SignOutAsync();
    return Results.Redirect("/account/login");
}).DisableAntiforgery();

app.MapIdentityNexus().RequireAuthorization();

app.Run();

static string RequiredConnection(string provider, string? connectionString) =>
    string.IsNullOrWhiteSpace(connectionString)
        ? throw new InvalidOperationException($"ConnectionStrings:IdentityNexus is required for {provider}.")
        : connectionString;

static string LocalReturn(string? url) =>
    !string.IsNullOrWhiteSpace(url)
    && Uri.IsWellFormedUriString(url, UriKind.Relative)
    && url.StartsWith('/')
    && !url.StartsWith("//", StringComparison.Ordinal)
        ? url
        : "/identity";
