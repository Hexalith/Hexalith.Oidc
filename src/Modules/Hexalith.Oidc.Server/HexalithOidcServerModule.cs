namespace Hexalith.Oidc.Server;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;

using Hexalith.Application.Modules.Modules;
using Hexalith.Extensions.Configuration;
using Hexalith.Extensions.Helpers;
using Hexalith.Oidc.Shared.Configurations;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Validators;

/// <summary>
/// Microsoft Entra ID server module.
/// </summary>
public sealed class HexalithOidcServerModule : IServerApplicationModule
{
    /// <inheritdoc/>
    public IEnumerable<string> Dependencies => [];

    /// <inheritdoc/>
    public string Description => "Microsoft Entra ID server module";

    /// <inheritdoc/>
    public string Id => "Hexalith.Oidc.Server";

    /// <inheritdoc/>
    public string Name => "Microsoft Entra ID server";

    /// <inheritdoc/>
    public int OrderWeight => 0;

    /// <inheritdoc/>
    public IEnumerable<Assembly> PresentationAssemblies => [GetType().Assembly];

    /// <inheritdoc/>
    public string Version => "1.0";

    /// <inheritdoc/>
    string IApplicationModule.Path => HexalithOidcServerModule.Path;

    private static string CookieScheme => "Cookies";

    private static string OidcScheme => "MicrosoftOidc";

    private static string Path => "hexalith/oidc";

    /// <summary>
    /// Adds services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        OidcSettings settings = configuration.GetSettings<OidcSettings>()
            ?? throw new InvalidOperationException($"Could not load settings section '{OidcSettings.ConfigurationName()}'");
        if (!settings.Enabled)
        {
            return;
        }

        _ = services.AddScoped<AuthenticationStateProvider, ServerPersistingAuthenticationStateProvider>();

        SettingsException<OidcSettings>.ThrowIfNullOrWhiteSpace(settings.ClientId);
        SettingsException<OidcSettings>.ThrowIfNullOrWhiteSpace(settings.ClientSecret);
        if (settings.OidcType != OidcType.MicrosoftEntraId)
        {
            SettingsException<OidcSettings>.ThrowIfNullOrWhiteSpace(settings.Authority);
        }

        // Add services to the container.
        _ = services.AddAuthentication(OidcScheme)
            .AddOpenIdConnect(OidcScheme, oidcOptions =>
            {
                // The OIDC handler must use a sign-in scheme capable of persisting
                // user credentials across requests.
                oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // The "openid" and "profile" scopes are required for the OIDC handler
                // and included by default. You should enable these scopes here if scopes
                // are provided by "Authentication:Schemes:MicrosoftOidc:Scope"
                // configuration because configuration may overwrite the scopes collection.
                oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);

                // SaveTokens is set to false by default because tokens aren't required
                // by the app to make additional external API requests.
                oidcOptions.SaveTokens = false;

                // The following paths must match the redirect and post logout redirect
                // paths configured when registering the application with the OIDC provider.
                // For Microsoft Entra ID, this is accomplished through the "Authentication"
                // blade of the application's registration in the Azure portal. Both the
                // signin and signout paths must be registered as Redirect URIs. The default
                // values are "/signin-oidc" and "/signout-callback-oidc".
                // Microsoft Identity currently only redirects back to the
                // SignedOutCallbackPath if authority is
                // https://login.microsoftonline.com/{TENANT ID}/v2.0/ as it is above.
                // You can use the "common" authority instead, and logout redirects back to
                // the Blazor app. For more information, see
                // https://github.com/AzureAD/microsoft-authentication-library-for-js/issues/5783
                oidcOptions.CallbackPath = new PathString("/signin-oidc");
                oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");

                // The RemoteSignOutPath is the "Front-channel logout URL" for remote single
                // sign-out. The default value is "/signout-oidc".
                oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");

                // The "offline_access" scope is required for the refresh token.
                oidcOptions.Scope.Add(OpenIdConnectScope.OfflineAccess);

                string tenant = string.IsNullOrWhiteSpace(settings.Tenant) ? "common" : settings.Tenant;
                oidcOptions.Authority = settings.OidcType == OidcType.MicrosoftEntraId
                    ? $"https://login.microsoftonline.com/{tenant}/v2.0/"
                    : settings.Authority;

                // Set the client identifier and secret for the app.
                oidcOptions.ClientId = settings.ClientId;
                oidcOptions.ClientSecret = settings.ClientSecret;

                // Setting ResponseType to "code" configures the OIDC handler to use
                // authorization code flow. Implicit grants and hybrid flows are unnecessary
                // in this mode. In a Microsoft Entra ID app registration, you don't need to
                // select either box for the authorization endpoint to return access tokens
                // or ID tokens. The OIDC handler automatically requests the appropriate
                // tokens using the code returned from the authorization endpoint.
                oidcOptions.ResponseType = OpenIdConnectResponseType.Code;

                // Many OIDC servers use "name" and "role" rather than the SOAP/WS-Fed
                // defaults in ClaimTypes. If you don't use ClaimTypes, mapping inbound
                // claims to ASP.NET Core's ClaimTypes isn't necessary.
                oidcOptions.MapInboundClaims = false;
                oidcOptions.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                oidcOptions.TokenValidationParameters.RoleClaimType = "role";

                if (settings.OidcType == OidcType.MicrosoftEntraId && string.IsNullOrWhiteSpace(settings.Tenant))
                {
                    // Many OIDC providers work with the default issuer validator, but the
                    // configuration must account for the issuer parameterized with "{TENANT ID}"
                    // returned by the "common" endpoint's /.well-known/openid-configuration
                    // For more information, see
                    // https://github.com/AzureAD/azure-activedirectory-identitymodel-extensions-for-dotnet/issues/1731
                    AadIssuerValidator microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
                    oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;
                }
            })
            .AddCookie(CookieScheme);

        // This attaches a cookie OnValidatePrincipal callback to get a new access token when the current one expires, and
        // reissue a cookie with the new access token saved inside. If the refresh fails, the user will be signed out.
        _ = services.ConfigureCookieOidcRefresh(CookieScheme, OidcScheme);

        _ = services.AddAuthorization();
    }

    /// <inheritdoc/>
    public void UseModule(object builder)
    {
        if (builder is not IEndpointRouteBuilder endpoints)
        {
            throw new ArgumentNullException(nameof(builder), $"The application object does not implement {nameof(IEndpointRouteBuilder)}.");
        }

        RouteGroupBuilder group = endpoints.MapGroup(Path);

        _ = group.MapGet("login", (string? returnUrl) => TypedResults.Challenge(GetAuthProperties(returnUrl)))
                .AllowAnonymous();

        // Sign out of the Cookie and OIDC handlers. If you do not sign out with the OIDC handler,
        // the user will automatically be signed back in the next time they visit a page that requires authentication
        // without being able to choose another account.
        _ = group.MapPost("logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(
                GetAuthProperties(returnUrl),
                [CookieScheme, OidcScheme]));
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        // TODO: Use HttpContext.Request.PathBase instead.
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}