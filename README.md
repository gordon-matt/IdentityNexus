<img src="https://github.com/gordon-matt/IdentityNexus/blob/master/_Misc/Logo.png" alt="Logo" width="350" />

A Blazor management UI for the ASP.NET Core Identity users and roles an application already has.
Drop it into any site that uses Identity with an Entity Framework store.

IdentityNexus does not create its own user tables and does not replace `UserManager` or `RoleManager`.

Users and roles use string keys. Passwords, security stamps, lockout and role assignment all go through the host's Identity managers, so the host's password rules and token providers stay in force.

## Screenshots

<img src="https://github.com/gordon-matt/IdentityNexus/blob/master/_Misc/Screenshots/Home.png" alt="Home Page" />

<img src="https://github.com/gordon-matt/IdentityNexus/blob/master/_Misc/Screenshots/Users.png" alt="Users Page" />

<img src="https://github.com/gordon-matt/IdentityNexus/blob/master/_Misc/Screenshots/Roles.png" alt="Roles Page" />

## What you can do

- Search and page users, and filter by role, lockout or an unconfirmed email.
- Create and edit users, including email, phone, confirmation flags and two-factor.
- Assign roles, edit claims, set a password, remove an external login, reset an authenticator, and delete a user.
- Create, rename and delete roles, including role claims.
- The signed-in account cannot delete itself or lock itself out.

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
    options.HomePath = "/";
    options.LogoutPath = "/account/logout";
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityNexus().RequireAuthorization();
```

`ApplicationUser` must derive from `IdentityUser`, and `ApplicationRole` from `IdentityRole`, with a
public parameterless constructor. Browse to `/identity`.

`Title` is the browser tab title. It is also the header text when it is not `IdentityNexus` and `LogoPath` is unset. `LogoPath` is a header image and takes precedence over `Title`. Use a root-relative path (`/logo.png`), an app-relative path (`~/logo.png`), or an absolute URL. With neither set, the built-in logo is shown.

`HomePath` adds a top-bar link back to the host site. `LogoutPath` adds the sign-out button. Set either, both, or neither. A relative home path is rooted so the UI's `<base href>` does not keep the link inside `/identity`.

## Tenants

A host that stores a tenant id on its user or role type can scope the UI to the current tenant. Call either method, or both. The selector is the property on the Identity type. The callback runs on each query, including inside the Blazor circuit, and returns `TenantKey<TKey>.None` when this request has no tenant. A value limits lists and lookups to that tenant and is written onto users or roles that IdentityNexus creates. An id from another tenant is treated as missing.

`TenantKey.OfNullable` accepts an `int?`, `Guid?`, or any other nullable value type. A reference-type id uses `TenantKey<string>.Of(id)` and `TenantKey<string>.None`.

```csharp
builder.Services.ScopeUsersToTenant<ApplicationUser, int?>(
    user => user.TenantId,
    services => TenantKey.OfNullable(services.GetRequiredService<AppTenant>().Id));

builder.Services.ScopeRolesToTenant<ApplicationRole, int?>(
    role => role.TenantId,
    services => TenantKey.OfNullable(services.GetRequiredService<AppTenant>().Id));
```

`MapIdentityNexus()` returns the route group, so secure it the same way as any other endpoint:

```csharp
app.MapIdentityNexus().RequireAuthorization("Administrators");
```

The UI is a separate Blazor circuit mounted at the base path. It uses the NetForge shell and the
host's `ForgeUiOptions.ThemeStorageKey`, so a theme change here follows the rest of the site.
`IdentityNexusOptions.Area` pins that theme when the UI is the back office (for example `Admin`)
even though it is mounted at `/identity` rather than `/Admin`. Leave `Area` unset to share the
public theme. Call `UseAuthentication` and `UseAuthorization` before `MapIdentityNexus`.
A host with no `.razor` files of its own still receives `blazor.web.js` when IdentityNexus is
installed as a package. A project reference should import `build/IdentityNexus.props`, which the
demo does.

## Demo

`IdentityNexus.Demo` is a small host that signs in with a cookie and serves the UI. Set
`Database:Provider` to `Sqlite`, `SqlServer`, `Npgsql` or `MySql`, and set
`ConnectionStrings:IdentityNexus` for anything other than the default SQLite file.

The seeded account is `admin@example.com` / `Password1!`.