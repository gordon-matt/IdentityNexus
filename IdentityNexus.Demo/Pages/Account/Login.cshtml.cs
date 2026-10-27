using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityNexus.Demo.Pages.Account;

public sealed class LoginModel(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : PageModel
{
    [BindProperty]
    public string Email { get; set; } = DemoData.AdminEmail;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public string? Error { get; set; }

    public void OnGet(string? returnUrl) => ReturnUrl = LocalReturn(returnUrl);

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl = LocalReturn(ReturnUrl);

        var user = await users.FindByEmailAsync(Email);
        if (user is null)
        {
            Error = "Those credentials were not accepted.";
            return Page();
        }

        var result = await signIn.PasswordSignInAsync(user, Password, isPersistent: true, lockoutOnFailure: true);
        return result.Succeeded
            ? LocalRedirect(ReturnUrl!)
            : Reject();
    }

    private PageResult Reject()
    {
        Error = "Those credentials were not accepted.";
        return Page();
    }

    private static string LocalReturn(string? url) =>
        !string.IsNullOrWhiteSpace(url)
        && Uri.IsWellFormedUriString(url, UriKind.Relative)
        && url.StartsWith('/')
        && !url.StartsWith("//", StringComparison.Ordinal)
            ? url
            : "/identity";
}