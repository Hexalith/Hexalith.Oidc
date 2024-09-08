namespace Hexalith.Oidc.Client;

using Hexalith.Application.Modules.Applications;
using Hexalith.Oidc.Shared;
using Hexalith.UI.Components.Modules;

/// <summary>
/// Represents a shared application.
/// </summary>
public class SharedApplication : HexalithSharedApplication
{
    /// <inheritdoc/>
    public override string HomePath => "hexalith";

    /// <inheritdoc/>
    public override string Id => "hexalithoidc";

    /// <inheritdoc/>
    public override string LoginPath => "authentication/login";

    /// <inheritdoc/>
    public override string LogoutPath => "authentication/logout";

    /// <inheritdoc/>
    public override string Name => "Hexalith Oidc";

    /// <inheritdoc/>
    public override IEnumerable<Type> SharedModules =>
    [
        typeof(HexalithOidcSharedModule), typeof(HexalithUIComponentsSharedModule)
    ];

    public override string Version => "1.0";
}