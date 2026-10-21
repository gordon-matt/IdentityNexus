using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace IdentityNexus.Services;

/// <summary>
/// Used when the host has not registered an authentication state provider. Interactive Server
/// replaces this with the circuit's provider, which is registered first.
/// </summary>
internal sealed class AnonymousAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
}
