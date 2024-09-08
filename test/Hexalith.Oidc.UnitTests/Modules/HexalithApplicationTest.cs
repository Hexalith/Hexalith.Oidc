// <copyright file="HexalithApplicationTest.cs" company="Hexalith SAS Paris France">
//     Copyright (c) Hexalith SAS Paris France. All rights reserved.
//     Licensed under the MIT license.
//     See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Oidc.UnitTests.Modules;

using FluentAssertions;

using Hexalith.Application.Modules.Applications;
using Hexalith.Oidc.Client;
using Hexalith.Oidc.Server;
using Hexalith.Oidc.Shared;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

public class HexalithApplicationTest
{
    [Fact]
    public void ClientServicesFromModulesShouldBeAdded()
    {
        ServiceCollection services = [];
        Mock<IConfiguration> configurationMock = new(MockBehavior.Strict);

        // Mock the configuration GetSection method
        _ = configurationMock
            .Setup(c => c.GetSection(It.IsAny<string>()))
            .Returns(new Mock<IConfigurationSection>().Object);

        HexalithApplication.AddClientServices(services, configurationMock.Object);

        // Check that the client module services have been added by checking if AuthenticationStateProvider has been added
        _ = services
            .Should()
            .ContainSingle(s => s.ServiceType == typeof(AuthenticationStateProvider));
    }

    [Fact]
    public void HexalithApplicationShouldReturnClientModuleTypes()
    {
        _ = HexalithApplication.Client.ClientModules
            .Should()
            .HaveCount(1);
        _ = HexalithApplication.Client.Modules
            .Should()
            .HaveCount(3);
        _ = HexalithApplication.Client.ClientModules
            .Should()
            .Contain(typeof(HexalithOidcClientModule));
        _ = HexalithApplication.Client.Modules
            .Should()
            .Contain(typeof(HexalithOidcSharedModule));
        _ = HexalithApplication.Client.Modules
            .Should()
            .Contain(typeof(HexalithOidcClientModule));
    }

    [Fact]
    public void HexalithApplicationShouldReturnServerModuleTypes()
    {
        _ = HexalithApplication.Server.ServerModules
            .Should()
            .HaveCount(1);
        _ = HexalithApplication.Server.Modules
            .Should()
            .HaveCount(3);
        _ = HexalithApplication.Server.ServerModules
            .Should()
            .Contain(typeof(HexalithOidcServerModule));
        _ = HexalithApplication.Server.Modules
            .Should()
            .Contain(typeof(HexalithOidcSharedModule));
    }
}