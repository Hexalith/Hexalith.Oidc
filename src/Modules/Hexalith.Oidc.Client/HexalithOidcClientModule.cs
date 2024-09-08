namespace Hexalith.Oidc.Client;

using System.Collections.Generic;
using System.Reflection;

using Hexalith.Application.Modules.Modules;
using Hexalith.Extensions.Helpers;
using Hexalith.Oidc.Shared.Configurations;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Microsoft Entra ID client module.
/// </summary>
public class HexalithOidcClientModule : IClientApplicationModule
{
    /// <inheritdoc/>
    public IEnumerable<string> Dependencies => [];

    /// <inheritdoc/>
    public string Description => "Microsoft Entra ID client module";

    /// <inheritdoc/>
    public string Id => "Hexalith.Oidc.Client";

    /// <inheritdoc/>
    public string Name => "Microsoft Entra ID client";

    /// <inheritdoc/>
    public int OrderWeight => 0;

    /// <inheritdoc/>
    public string Path => "hexalith/microsoftentraid";

    /// <inheritdoc/>
    public IEnumerable<Assembly> PresentationAssemblies => [GetType().Assembly];

    /// <inheritdoc/>
    public string Version => "1.0";

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

        _ = services.AddScoped<AuthenticationStateProvider, ClientPersistentAuthenticationStateProvider>();
    }

    /// <inheritdoc/>
    public void UseModule(object builder)
    {
    }
}