using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.EntityFramework.Options;
using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using IdentityServer3.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;
using Xunit;
using Client = IdentityServer3.EntityFramework.Entities.Client;

namespace Rsk.IdentityServer.Migration.Tests
{
    public class MigratorTests
    {
        // Empty test database
        private const string IdentityServer3DbContext = "Server=.;User Id=Identity;Password=Password123!;Database=IdentityServer3Db;";
        
        private readonly Client testClient = new Client
        {
            AccessTokenType = AccessTokenType.Jwt,
            AbsoluteRefreshTokenLifetime = 90000,
            AccessTokenLifetime = 600,
            AllowAccessToAllGrantTypes = false,
            AllowAccessToAllScopes = true,
            AllowAccessTokensViaBrowser = false,
            AllowClientCredentialsOnly = false,
            AllowRememberConsent = true,
            AllowedCorsOrigins = new List<ClientCorsOrigin>() { new() { Origin = "http://localhost:5001" } },
            AllowedScopes = new List<ClientScope> { new() { Scope = "openid" }, new() { Scope = "profile" }, new() { Scope = "api1" }, new ClientScope { Scope = "offline_access" } },
            AlwaysSendClientClaims = true,
            AuthorizationCodeLifetime = 300,
            Claims = new List<ClientClaim>() { new() { Type = "test", Value = "123" } },
            ClientId = Guid.NewGuid().ToString(),
            ClientName = Guid.NewGuid().ToString(),
            ClientSecrets = new List<ClientSecret>() { new() { Value = "isthisasecret?yesitis".Sha256() } },
            ClientUri = "http://localhost:5001/policy",
            EnableLocalLogin = true,
            Enabled = true,
            Flow = Flows.Hybrid,
            IdentityProviderRestrictions = new List<ClientIdPRestriction> { new() { Provider = "google" }, new() { Provider = "local" } },
            IdentityTokenLifetime = 300,
            IncludeJwtId = false,
            LogoUri = "http://localhost:5001/face.jpg",
            LogoutUri = "http://locahost:5001/logout",
            LogoutSessionRequired = false,
            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri>() { new() { Uri = "http://localhost:5001/" } },
            PrefixClientClaims = true,
            RedirectUris = new List<ClientRedirectUri> { new() { Uri = "http://localhost:5001/cb" } },
            RefreshTokenExpiration = TokenExpiration.Absolute,
            RefreshTokenUsage = TokenUsage.OneTimeOnly,
            RequireConsent = true,
            RequireSignOutPrompt = false,
            SlidingRefreshTokenLifetime = 0,
            UpdateAccessTokenOnRefresh = true
        };

        private readonly IdentityServer3.EntityFramework.Entities.Scope resourceScope = new()
        {
            Name = Guid.NewGuid().ToString(),
            DisplayName = Guid.NewGuid().ToString(),
            Type = 1,
            Description = Guid.NewGuid().ToString(),
            AllowUnrestrictedIntrospection = false,
            ClaimsRule = Guid.NewGuid().ToString(),
            Emphasize = true,
            Enabled = true,
            IncludeAllClaimsForUser = true,
            Required = true,
            ShowInDiscoveryDocument = true,
            ScopeClaims = new List<IdentityServer3.EntityFramework.Entities.ScopeClaim> { new() { Name = "sub" } },
            ScopeSecrets = new List<ScopeSecret> { new ScopeSecret { Value = Guid.NewGuid().ToString().Sha256() } }
        };

        private readonly IdentityServer3.EntityFramework.Entities.Scope identityScope = new()
        {
            Name = Guid.NewGuid().ToString(),
            DisplayName = Guid.NewGuid().ToString(),
            Type = 0,
            Description = Guid.NewGuid().ToString(),
            AllowUnrestrictedIntrospection = false,
            ClaimsRule = Guid.NewGuid().ToString(),
            Emphasize = true,
            Enabled = true,
            IncludeAllClaimsForUser = true,
            Required = true,
            ShowInDiscoveryDocument = true,
            ScopeClaims = new List<IdentityServer3.EntityFramework.Entities.ScopeClaim> { new() { Name = "sub"} },
            ScopeSecrets = new List<ScopeSecret> { new() { Value = Guid.NewGuid().ToString().Sha256() } }
        };
        
        public MigratorTests()
        {
            using (var context = new ClientConfigurationDbContext(IdentityServer3DbContext))
            {
                context.Clients.Add(testClient);
                context.SaveChanges();
            }

            using (var context = new ScopeConfigurationDbContext(IdentityServer3DbContext))
            {
                context.Scopes.Add(resourceScope);
                context.Scopes.Add(identityScope);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task WhenClientMigrated_ExpectCorrectValues()
        {
            var clientsContext = new ClientConfigurationDbContext(IdentityServer3DbContext);
            var scopesContext = new ScopeConfigurationDbContext(IdentityServer3DbContext);

            var dbContextOptions = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseInMemoryDatabase(nameof(WhenIdentityScopeMigrated_ExpectCorrectValues))
                .Options;

            var ids4Context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions());

            var sut = new Migrator(
                new EntityFrameworkClientReader(clientsContext),
                new EntityFrameworkScopeReader(scopesContext),
                new EntityFrameworkClientWriter(ids4Context),
                new EntityFrameworkApiResourceWriter(ids4Context),
                new EntityFrameworkIdentityResourceWriter(ids4Context));

            await sut.Migrate();

            Duende.IdentityServer.EntityFramework.Entities.Client migratedEfClient;
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                migratedEfClient = context.Clients
                .Include(x => x.AllowedCorsOrigins)
                .Include(x => x.AllowedGrantTypes)
                .Include(x => x.AllowedScopes)
                .Include(x => x.Claims)
                .Include(x => x.ClientSecrets)
                .Include(x => x.IdentityProviderRestrictions)
                .Include(x => x.PostLogoutRedirectUris)
                .Include(x => x.Properties)
                .Include(x => x.RedirectUris)
                .FirstOrDefault(x => x.ClientId == testClient.ClientId);
            }

            migratedEfClient.Should().NotBeNull();
            var migratedClient = migratedEfClient.ToModel();

            migratedClient.AbsoluteRefreshTokenLifetime.Should().Be(testClient.AbsoluteRefreshTokenLifetime);
            migratedClient.AccessTokenLifetime.Should().Be(testClient.AccessTokenLifetime);
            migratedClient.AccessTokenType.Should().Be(Duende.IdentityServer.Models.AccessTokenType.Jwt);
            migratedClient.AllowAccessTokensViaBrowser.Should().Be(testClient.AllowAccessTokensViaBrowser);
            migratedClient.AllowRememberConsent.Should().Be(testClient.AllowRememberConsent);
            migratedClient.AllowedCorsOrigins.Should().Contain(testClient.AllowedCorsOrigins.FirstOrDefault()?.Origin);
            migratedClient.AllowedScopes.Should().Contain(new List<string> { "openid", "profile", "api1" });
            migratedClient.AlwaysSendClientClaims.Should().Be(testClient.AlwaysSendClientClaims);
            migratedClient.AuthorizationCodeLifetime.Should().Be(testClient.AuthorizationCodeLifetime);
            migratedClient.ClientId.Should().Be(testClient.ClientId);
            migratedClient.ClientName.Should().Be(testClient.ClientName);
            migratedClient.ClientUri.Should().Be(testClient.ClientUri);
            migratedClient.EnableLocalLogin.Should().Be(testClient.EnableLocalLogin);
            migratedClient.Enabled.Should().Be(testClient.Enabled);
            migratedClient.FrontChannelLogoutUri.Should().Be(testClient.LogoutUri);
            migratedClient.FrontChannelLogoutSessionRequired.Should().Be(testClient.LogoutSessionRequired);
            migratedClient.IdentityProviderRestrictions.Should()
                .BeEquivalentTo(testClient.IdentityProviderRestrictions.Select(x=>x.Provider));
            migratedClient.IdentityTokenLifetime.Should().Be(testClient.IdentityTokenLifetime);
            migratedClient.IncludeJwtId.Should().Be(testClient.IncludeJwtId);
            migratedClient.LogoUri.Should().Be(testClient.LogoUri);
            migratedClient.PostLogoutRedirectUris.Should().BeEquivalentTo(testClient.PostLogoutRedirectUris.Select(x=>x.Uri));
            migratedClient.RedirectUris.Should().BeEquivalentTo(testClient.RedirectUris.Select(x=>x.Uri));
            migratedClient.RefreshTokenExpiration.Should().Be(Duende.IdentityServer.Models.TokenExpiration.Absolute);
            migratedClient.RefreshTokenUsage.Should().Be(Duende.IdentityServer.Models.TokenUsage.OneTimeOnly);
            migratedClient.RequireConsent.Should().Be(testClient.RequireConsent);
            migratedClient.SlidingRefreshTokenLifetime.Should().Be(testClient.SlidingRefreshTokenLifetime);
            migratedClient.UpdateAccessTokenClaimsOnRefresh.Should()
                .Be(testClient.UpdateAccessTokenOnRefresh);

            migratedClient.ClientClaimsPrefix.Should().Be("client_");
            migratedClient.AllowedScopes.Should().NotContain("offline_access");
            migratedClient.AllowOfflineAccess.Should().BeTrue();

            foreach (var testClientSecret in testClient.ClientSecrets)
            {
                var migratedSecret =
                    migratedClient.ClientSecrets.FirstOrDefault(x => x.Value == testClientSecret.Value);
                migratedSecret.Should().NotBeNull();

                migratedSecret?.Type.Should().Be(testClientSecret.Type);
                migratedSecret?.Description.Should().Be(testClientSecret.Description);
                if (testClientSecret.Expiration != null)
                    migratedSecret?.Expiration?.Ticks.Should().Be(testClientSecret.Expiration.Value.Ticks);
            }

        }

        [Fact]
        public async Task WhenApiScopeMigrated_ExpectCorrectValues()
        {
            var clientsContext = new ClientConfigurationDbContext(IdentityServer3DbContext);
            var scopesContext = new ScopeConfigurationDbContext(IdentityServer3DbContext);

            var dbContextOptions = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseInMemoryDatabase(nameof(WhenIdentityScopeMigrated_ExpectCorrectValues))
                .Options;

            var ids4Context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions());

            var sut = new Migrator(
                new EntityFrameworkClientReader(clientsContext),
                new EntityFrameworkScopeReader(scopesContext),
                new EntityFrameworkClientWriter(ids4Context),
                new EntityFrameworkApiResourceWriter(ids4Context),
                new EntityFrameworkIdentityResourceWriter(ids4Context));

            await sut.Migrate();

            Duende.IdentityServer.EntityFramework.Entities.ApiResource migratedEfResource;
            Duende.IdentityServer.EntityFramework.Entities.ApiScope migratedScope;
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                migratedEfResource = context.ApiResources
                    .Include(x => x.Scopes)
                    .Include(x => x.Secrets)
                    .Include(x => x.UserClaims)
                    .FirstOrDefault(x => x.Name == resourceScope.Name);

                migratedScope = context.ApiScopes
                    .Include(x => x.UserClaims)
                    .Include(x => x.Properties)
                    .FirstOrDefault(x => x.Name == resourceScope.Name);
            }

            migratedEfResource.Should().NotBeNull();
            var migratedResource = migratedEfResource.ToModel();

            migratedResource.Should().NotBeNull();
            migratedResource.Name.Should().Be(resourceScope.Name);
            migratedResource.DisplayName.Should().Be(resourceScope.DisplayName);
            migratedResource.Description.Should().Be(resourceScope.Description);
            migratedResource.Enabled.Should().Be(resourceScope.Enabled);

            var migratedResourceScope = migratedScope.ToModel();
            migratedResourceScope.Description.Should().Be(resourceScope.Description);
            migratedResourceScope.DisplayName.Should().Be(resourceScope.DisplayName);
            migratedResourceScope.Emphasize.Should().Be(resourceScope.Emphasize);
            migratedResourceScope.Name.Should().Be(resourceScope.Name);
            migratedResourceScope.Required.Should().Be(resourceScope.Required);
            migratedResourceScope.ShowInDiscoveryDocument.Should().Be(resourceScope.ShowInDiscoveryDocument);
            migratedResourceScope.UserClaims.Should().BeEmpty();

            foreach (var scopeClaim in resourceScope.ScopeClaims)
                migratedResource.UserClaims.Should().Contain(x => x == scopeClaim.Name);

            foreach (var testResourceSecret in resourceScope.ScopeSecrets)
            {
                var migratedSecret = migratedResource.ApiSecrets.FirstOrDefault(x => x.Value == testResourceSecret.Value);
                migratedSecret.Should().NotBeNull();

                migratedSecret?.Type.Should().Be(testResourceSecret.Type);
                migratedSecret?.Description.Should().Be(testResourceSecret.Description);
                if (testResourceSecret.Expiration != null)
                    migratedSecret?.Expiration?.Ticks.Should().Be(testResourceSecret.Expiration.Value.Ticks);
            }
        }

        [Fact]
        public async Task WhenIdentityScopeMigrated_ExpectCorrectValues()
        {
            var clientsContext = new ClientConfigurationDbContext(IdentityServer3DbContext);
            var scopesContext = new ScopeConfigurationDbContext(IdentityServer3DbContext);

            var dbContextOptions = new DbContextOptionsBuilder<ConfigurationDbContext>()
                .UseInMemoryDatabase(nameof(WhenIdentityScopeMigrated_ExpectCorrectValues))
                .Options;

            var ids4Context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions());

            var sut = new Migrator(
                new EntityFrameworkClientReader(clientsContext),
                new EntityFrameworkScopeReader(scopesContext),
                new EntityFrameworkClientWriter(ids4Context),
                new EntityFrameworkApiResourceWriter(ids4Context),
                new EntityFrameworkIdentityResourceWriter(ids4Context));

            await sut.Migrate();

            Duende.IdentityServer.EntityFramework.Entities.IdentityResource migratedEfResource;
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                migratedEfResource = context.IdentityResources
                    .Include(x => x.UserClaims)
                    .FirstOrDefault(x => x.Name == identityScope.Name);
            }

            migratedEfResource.Should().NotBeNull();
            var migratedResource = migratedEfResource.ToModel();

            migratedResource.Should().NotBeNull();
            migratedResource.Name.Should().Be(identityScope.Name);
            migratedResource.DisplayName.Should().Be(identityScope.DisplayName);
            migratedResource.Description.Should().Be(identityScope.Description);
            migratedResource.Enabled.Should().Be(identityScope.Enabled);
            migratedResource.Emphasize.Should().Be(identityScope.Emphasize);
            migratedResource.ShowInDiscoveryDocument.Should().Be(identityScope.ShowInDiscoveryDocument);
            migratedResource.Required.Should().Be(identityScope.Required);

            foreach (var scopeClaim in identityScope.ScopeClaims)
                migratedResource.UserClaims.Should().Contain(x => x == scopeClaim.Name);
        }
    }
}