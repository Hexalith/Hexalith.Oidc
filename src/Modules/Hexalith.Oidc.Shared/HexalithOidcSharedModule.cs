namespace Hexalith.Oidc.Shared;

using System.Collections.Generic;
using System.Reflection;

using Hexalith.Application.Modules.Modules;
using Hexalith.Extensions.Configuration;
using Hexalith.Extensions.Helpers;
using Hexalith.Oidc.Shared.Configurations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Microsoft Entra ID shared module.
/// </summary>
public class HexalithOidcSharedModule : ISharedApplicationModule
{
    /// <inheritdoc/>
    public IEnumerable<string> Dependencies => [];

    /// <inheritdoc/>
    public string Description => "Hexalith Open ID connect shared module";

    /// <inheritdoc/>
    public string Id => "Hexalith.Oidc.Shared";

    /// <inheritdoc/>
    public string Name => "Hexalith OIDC shared";

    /// <inheritdoc/>
    public int OrderWeight => 0;

    /// <inheritdoc/>
    public string Path => "hexalith/oidc";

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

        _ = services
            .AddAuthorizationCore()
            .AddCascadingAuthenticationState()
            .ConfigureSettings<OidcSettings>(configuration);
    }

    /// <inheritdoc/>
    public void UseModule(object builder)
    {
    }
}