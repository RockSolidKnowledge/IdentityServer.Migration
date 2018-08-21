using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;
using Xunit;

namespace Rsk.IdentityServer.Migration.Tests
{
    public class MigratorTests : IDisposable
    {
        private const string ClientsConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=migration.test.clients;Integrated Security=SSPI;";
        private const string ScopesConnectionString = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=migration.test.scopes;Integrated Security=SSPI;";
        private readonly DbOptions options = new DbOptions
        {
            IdentityServer3ClientsConnectionString = ClientsConnectionString,
            IdentityServer3ScopesConnectionString = ScopesConnectionString
        };

        private readonly DbContextOptions<ConfigurationDbContext> dbContextOptions = new DbContextOptionsBuilder<ConfigurationDbContext>()
            .UseInMemoryDatabase("IdentityServer")
            .Options;

        private readonly Client testClient = new Client
        {
            AccessTokenType = AccessTokenType.Jwt,
            AbsoluteRefreshTokenLifetime = 90000,
            AccessTokenLifetime = 600,
            AllowAccessToAllCustomGrantTypes = false,
            AllowAccessToAllScopes = true,
            AllowAccessTokensViaBrowser = false,
            AllowClientCredentialsOnly = false,
            AllowRememberConsent = true,
            AllowedCorsOrigins = {"http://localhost:5001"},
            AllowedScopes = {"openid", "profile", "api1", "offline_access"},
            AlwaysSendClientClaims = true,
            AuthorizationCodeLifetime = 300,
            Claims = {new Claim("test", "123")},
            ClientId = Guid.NewGuid().ToString(),
            ClientName = Guid.NewGuid().ToString(),
            ClientSecrets = {new Secret("isthisasecret?yesitis".Sha256())},
            ClientUri = "http://localhost:5001/policy",
            EnableLocalLogin = true,
            Enabled = true,
            Flow = Flows.Hybrid,
            IdentityProviderRestrictions = {"google", "local"},
            IdentityTokenLifetime = 300,
            IncludeJwtId = false,
            LogoUri = "http://localhost:5001/face.jpg",
            LogoutUri = "http://locahost:5001/logout",
            LogoutSessionRequired = false,
            PostLogoutRedirectUris = {"http://localhost:5001/"},
            PrefixClientClaims = true,
            RedirectUris = {"http://localhost:5001/cb"},
            RefreshTokenExpiration = TokenExpiration.Absolute,
            RefreshTokenUsage = TokenUsage.OneTimeOnly,
            RequireConsent = true,
            RequireSignOutPrompt = false,
            SlidingRefreshTokenLifetime = 0,
            UpdateAccessTokenClaimsOnRefresh = true
        };

        private readonly Scope resourceScope = new Scope
        {
            Name = Guid.NewGuid().ToString(),
            DisplayName = Guid.NewGuid().ToString(),
            Type = ScopeType.Resource,
            Description = Guid.NewGuid().ToString(),
            AllowUnrestrictedIntrospection = false,
            ClaimsRule = Guid.NewGuid().ToString(),
            Emphasize = true,
            Enabled = true,
            IncludeAllClaimsForUser = true,
            Required = true,
            ShowInDiscoveryDocument = true,
            Claims = {new ScopeClaim("sub")},
            ScopeSecrets = {new Secret(Guid.NewGuid().ToString().Sha256(), Guid.NewGuid().ToString())}
        };

        private readonly Scope identityScope = new Scope
        {
            Name = Guid.NewGuid().ToString(),
            DisplayName = Guid.NewGuid().ToString(),
            Type = ScopeType.Identity,
            Description = Guid.NewGuid().ToString(),
            AllowUnrestrictedIntrospection = false,
            ClaimsRule = Guid.NewGuid().ToString(),
            Emphasize = true,
            Enabled = true,
            IncludeAllClaimsForUser = true,
            Required = true,
            ShowInDiscoveryDocument = true,
            Claims = { new ScopeClaim("sub") },
            ScopeSecrets = { new Secret(Guid.NewGuid().ToString().Sha256(), Guid.NewGuid().ToString())}
        };

        public MigratorTests()
        {
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<ClientConfigurationDbContext>());
            System.Data.Entity.Database.SetInitializer(new System.Data.Entity.CreateDatabaseIfNotExists<ScopeConfigurationDbContext>());

            using (var context = new ClientConfigurationDbContext(ClientsConnectionString))
            {
                context.Clients.Add(testClient.ToEntity());
                context.SaveChanges();
            }

            using (var context = new ScopeConfigurationDbContext(ScopesConnectionString))
            {
                context.Scopes.Add(resourceScope.ToEntity());
                context.Scopes.Add(identityScope.ToEntity());
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task WhenClientMigrated_ExpectCorrectValues()
        {
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                var sut = new Migrator(
                    new EntityFrameworkClientReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkScopeReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkClientWriter(context),
                    new EntityFrameworkApiResourceWriter(context),
                    new EntityFrameworkIdentityResourceWriter(context));

                await sut.Migrate();
            }

            IdentityServer4.EntityFramework.Entities.Client migratedEfClient;
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
            migratedClient.AccessTokenType.Should().Be(IdentityServer4.Models.AccessTokenType.Jwt);
            migratedClient.AllowAccessTokensViaBrowser.Should().Be(testClient.AllowAccessTokensViaBrowser);
            migratedClient.AllowRememberConsent.Should().Be(testClient.AllowRememberConsent);
            migratedClient.AllowedCorsOrigins.Should().BeEquivalentTo(testClient.AllowedCorsOrigins);
            migratedClient.AllowedScopes.Should().Contain(new List<string> {"openid", "profile", "api1"});
            migratedClient.AlwaysSendClientClaims.Should().Be(testClient.AlwaysSendClientClaims);
            migratedClient.AuthorizationCodeLifetime.Should().Be(testClient.AuthorizationCodeLifetime);
            migratedClient.ClientId.Should().Be(testClient.ClientId);
            migratedClient.ClientName.Should().Be(testClient.ClientName);
            migratedClient.ClientUri.Should().Be(testClient.ClientUri);
            migratedClient.EnableLocalLogin.Should().Be(testClient.EnableLocalLogin);
            migratedClient.Enabled.Should().Be(testClient.Enabled);
            migratedClient.FrontChannelLogoutUri.Should().Be(testClient.LogoutUri);
            migratedClient.FrontChannelLogoutSessionRequired.Should().Be(testClient.LogoutSessionRequired);
            migratedClient.IdentityProviderRestrictions.Should().BeEquivalentTo(testClient.IdentityProviderRestrictions);
            migratedClient.IdentityTokenLifetime.Should().Be(testClient.IdentityTokenLifetime);
            migratedClient.IncludeJwtId.Should().Be(testClient.IncludeJwtId);
            migratedClient.LogoUri.Should().Be(testClient.LogoUri);
            migratedClient.PostLogoutRedirectUris.Should().BeEquivalentTo(testClient.PostLogoutRedirectUris);
            migratedClient.RedirectUris.Should().BeEquivalentTo(testClient.RedirectUris);
            migratedClient.RefreshTokenExpiration.Should().Be(IdentityServer4.Models.TokenExpiration.Absolute);
            migratedClient.RefreshTokenUsage.Should().Be(IdentityServer4.Models.TokenUsage.OneTimeOnly);
            migratedClient.RequireConsent.Should().Be(testClient.RequireConsent);
            migratedClient.SlidingRefreshTokenLifetime.Should().Be(testClient.SlidingRefreshTokenLifetime);
            migratedClient.UpdateAccessTokenClaimsOnRefresh.Should().Be(testClient.UpdateAccessTokenClaimsOnRefresh);

            migratedClient.ClientClaimsPrefix.Should().Be("client_");
            migratedClient.AllowedScopes.Should().NotContain("offline_access");
            migratedClient.AllowOfflineAccess.Should().BeTrue();

            foreach (var testClientSecret in testClient.ClientSecrets)
            {
                var migratedSecret = migratedClient.ClientSecrets.FirstOrDefault(x => x.Value == testClientSecret.Value);
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
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                var sut = new Migrator(
                    new EntityFrameworkClientReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkScopeReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkClientWriter(context),
                    new EntityFrameworkApiResourceWriter(context),
                    new EntityFrameworkIdentityResourceWriter(context));

                await sut.Migrate();
            }

            IdentityServer4.EntityFramework.Entities.ApiResource migratedEfResource;
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                migratedEfResource = context.ApiResources
                    .Include(x => x.Scopes)
                    .Include(x => x.Secrets)
                    .Include(x => x.UserClaims)
                    .FirstOrDefault(x => x.Name == resourceScope.Name);
            }

            migratedEfResource.Should().NotBeNull();
            var migratedResource = migratedEfResource.ToModel();

            migratedResource.Should().NotBeNull();
            migratedResource.Name.Should().Be(resourceScope.Name);
            migratedResource.DisplayName.Should().Be(resourceScope.DisplayName);
            migratedResource.Description.Should().Be(resourceScope.Description);
            migratedResource.Enabled.Should().Be(resourceScope.Enabled);

            var migratedResourceScope = migratedResource.Scopes.Single();
            migratedResourceScope.Description.Should().Be(resourceScope.Description);
            migratedResourceScope.DisplayName.Should().Be(resourceScope.DisplayName);
            migratedResourceScope.Emphasize.Should().Be(resourceScope.Emphasize);
            migratedResourceScope.Name.Should().Be(resourceScope.Name);
            migratedResourceScope.Required.Should().Be(resourceScope.Required);
            migratedResourceScope.ShowInDiscoveryDocument.Should().Be(resourceScope.ShowInDiscoveryDocument);
            migratedResourceScope.UserClaims.Should().BeEmpty();

            foreach (var scopeClaim in resourceScope.Claims)
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
            using (var context = new ConfigurationDbContext(dbContextOptions, new ConfigurationStoreOptions()))
            {
                var sut = new Migrator(
                    new EntityFrameworkClientReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkScopeReader(new OptionsWrapper<DbOptions>(options)),
                    new EntityFrameworkClientWriter(context),
                    new EntityFrameworkApiResourceWriter(context),
                    new EntityFrameworkIdentityResourceWriter(context));

                await sut.Migrate();
            }

            IdentityServer4.EntityFramework.Entities.IdentityResource migratedEfResource;
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

            foreach (var scopeClaim in identityScope.Claims)
                migratedResource.UserClaims.Should().Contain(x => x == scopeClaim.Name);
        }

        public void Dispose()
        {
            using (var context = new ClientConfigurationDbContext(ClientsConnectionString))
                context.Database.Delete();
            using (var context = new ScopeConfigurationDbContext(ScopesConnectionString))
                context.Database.Delete();
        }
    }
}