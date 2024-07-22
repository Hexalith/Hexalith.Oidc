// <copyright file="HexalithApplicationTest.cs" company="Fiveforty SAS Paris France">
//     Copyright (c) Fiveforty SAS Paris France. All rights reserved.
//     Licensed under the MIT license.
//     See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Oidc.UnitTests.Modules;

using FluentAssertions;

using Hexalith.Application.Modules.Applications;
using Hexalith.Oidc.Client;
using Hexalith.Oidc.Server;
using Hexalith.Oidc.Shared;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

public class HexalithApplicationTest
{
    [Fact]
    public void ClientServicesFromModulesShouldBeAdded()
    {
        ServiceCollection services = [];
        Mock<IConfiguration> configurationMock = new();

        HexalithApplication.AddClientServices(services, configurationMock.Object);

        // Check that the services have been added
        _ = services
            .Should()
            .HaveCount(1);
    }

    [Fact]
    public void HexalithApplicationShouldReturnClientModuleTypes()
    {
        _ = HexalithApplication.Client.ClientModules
            .Should()
            .HaveCount(1);
        _ = HexalithApplication.Client.Modules
            .Should()
            .HaveCount(2);
        _ = HexalithApplication.Client.ClientModules
            .Should()
            .Contain(typeof(HexalithOidcClientModule));
        _ = HexalithApplication.Client.Modules
            .Should()
            .Contain(typeof(HexalithOidcSharedModule));
    }

    [Fact]
    public void HexalithApplicationShouldReturnServerModuleTypes()
    {
        _ = HexalithApplication.Server.ServerModules
            .Should()
            .HaveCount(1);
        _ = HexalithApplication.Server.Modules
            .Should()
            .HaveCount(2);
        _ = HexalithApplication.Server.ServerModules
            .Should()
            .Contain(typeof(HexalithOidcServerModule));
        _ = HexalithApplication.Server.Modules
            .Should()
            .Contain(typeof(HexalithOidcSharedModule));
    }

    [Fact]
    public void ServerServicesFromModulesShouldBeAdded()
    {
        ServiceCollection services = [];
        Mock<IConfiguration> configurationMock = new();

        HexalithApplication.AddServerServices(services, configurationMock.Object);

        // Check that the services have been added
        _ = services
            .Should()
            .HaveCount(1);
    }
}