namespace Hexalith.Oidc.Server;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

internal static class CookieOidcServiceCollectionExtensions
{
    public static IServiceCollection ConfigureCookieOidcRefresh(this IServiceCollection services, string cookieScheme, string oidcScheme)
    {
        _ = services.AddSingleton<CookieOidcRefresher>();
        _ = services.AddOptions<CookieAuthenticationOptions>(cookieScheme).Configure<CookieOidcRefresher>((cookieOptions, refresher) => cookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, oidcScheme));
        _ = services.AddOptions<OpenIdConnectOptions>(oidcScheme).Configure(oidcOptions =>
        {
            // Request a refresh_token.
            oidcOptions.Scope.Add("offline_access");

            // Store the refresh_token.
            oidcOptions.SaveTokens = true;
        });
        return services;
    }
}