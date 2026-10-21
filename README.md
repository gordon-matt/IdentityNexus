A Blazor management UI for the ASP.NET Core Identity users and roles an application already has.
Drop it into Mantle, Inferno, or any other site that uses Identity with an Entity Framework store.
IdentityNexus does not create its own user tables and does not replace `UserManager` or `RoleManager`.

Users and roles use string keys, which is the Identity default and what Mantle and Inferno use.
Passwords, security stamps, lockout and role assignment all go through the host's Identity managers,
so the host's password rules and token providers stay in force.

## What you can do

- Search and page users, and filter by role, lockout or an unconfirmed email.
- Create and edit users, including email, phone, confirmation flags and two-factor.
- Assign roles, edit claims, set a password, remove an external login, reset an authenticator, and delete a user.
- Create, rename and delete roles, including role claims.
- The signed-in account cannot delete itself.

## Add it to an existing site

The host registers Identity first, against whichever database it already uses. SQLite, SQL Server,
PostgreSQL and MySQL all work, because queries go through the host's Entity Framework store.

```csharp
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityNexus<ApplicationUser, ApplicationRole>(options =>
{
    options.LogoutPath = "/account/logout";
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityNexus().RequireAuthorization();
```

`ApplicationUser` must derive from `IdentityUser`, and `ApplicationRole` from `IdentityRole`, with a
public parameterless constructor. Inferno's `ApplicationUser` and Mantle's user type both qualify.
Browse to `/identity`.

`MapIdentityNexus()` returns the route group, so secure it the same way as any other endpoint:

```csharp
app.MapIdentityNexus().RequireAuthorization("Administrators");
```

The UI is a separate Blazor circuit mounted at the base path, with its own stylesheet, so it does
not pick up the host's CSS. Call `UseAuthentication` and `UseAuthorization` before `MapIdentityNexus`.
A host with no `.razor` files of its own still receives `blazor.web.js` when IdentityNexus is
installed as a package. A project reference should import `build/IdentityNexus.props`, which the
demo does.

## Demo

`IdentityNexus.Demo` is a small host that signs in with a cookie and serves the UI. Set
`Database:Provider` to `Sqlite`, `SqlServer`, `Npgsql` or `MySql`, and set
`ConnectionStrings:IdentityNexus` for anything other than the default SQLite file.

The seeded account is `admin@example.com` / `Password1!`.
