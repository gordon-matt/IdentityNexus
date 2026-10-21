using System.Net;

namespace IdentityNexus.Demo;

internal static class LoginPage
{
    public static string Html(string? returnUrl, string? error = null)
    {
        string safeReturn = WebUtility.HtmlEncode(returnUrl ?? "/identity");
        string alert = string.IsNullOrWhiteSpace(error)
            ? ""
            : $"""<p class="error">{WebUtility.HtmlEncode(error)}</p>""";

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1" />
                <title>Sign in</title>
                <style>
                    :root { color-scheme: light dark; }
                    body { margin: 0; min-height: 100vh; display: grid; place-items: center; font: 14px/1.45 system-ui, sans-serif; background: #f6f7f9; color: #1a1e24; }
                    form { width: min(380px, calc(100vw - 2rem)); padding: 1.4rem; border: 1px solid #e2e5ea; border-radius: 8px; background: #fff; }
                    h1 { margin: 0 0 0.3rem; font-size: 20px; }
                    p { margin: 0 0 1rem; color: #6b7380; }
                    label { display: block; margin-bottom: 0.75rem; }
                    span { display: block; margin-bottom: 0.3rem; color: #6b7380; font-size: 12.5px; }
                    input { width: 100%; box-sizing: border-box; height: 36px; padding: 0 0.7rem; border: 1px solid #e2e5ea; border-radius: 6px; }
                    button { height: 36px; padding: 0 0.9rem; border: 0; border-radius: 6px; background: #4f46e5; color: white; font: inherit; cursor: pointer; }
                    .error { color: #dc2626; }
                    @media (prefers-color-scheme: dark) {
                        body { background: #0d1015; color: #e5e8ee; }
                        form { background: #151920; border-color: #283040; }
                        input { background: #0d1015; color: inherit; border-color: #283040; }
                    }
                </style>
            </head>
            <body>
                <form method="post" action="/account/login">
                    <h1>Sign in</h1>
                    <p>Demo account: {{DemoData.AdminEmail}} / {{DemoData.AdminPassword}}</p>
                    {{alert}}
                    <input type="hidden" name="returnUrl" value="{{safeReturn}}" />
                    <label>
                        <span>Email</span>
                        <input name="email" type="email" autocomplete="username" required value="{{DemoData.AdminEmail}}" />
                    </label>
                    <label>
                        <span>Password</span>
                        <input name="password" type="password" autocomplete="current-password" required />
                    </label>
                    <button type="submit">Sign in</button>
                </form>
            </body>
            </html>
            """;
    }
}
