using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;
using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework.Entities;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;
using AccessTokenType = IdentityServer3.Core.Models.AccessTokenType;
using Client = IdentityServer3.EntityFramework.Entities.Client;
using TokenExpiration = IdentityServer3.Core.Models.TokenExpiration;
using TokenUsage = IdentityServer3.Core.Models.TokenUsage;

namespace Rsk.IdentityServer.Migration.Tests.Mappers
{
    public class ClientMapperTests
    {
        [Fact]
        public void GivenClient_ExpectClientPropertiesMappedCorrectly()
        {
            var random = new Random();
            var client = new Client
            {
                AbsoluteRefreshTokenLifetime = random.Next(),
                AccessTokenLifetime = random.Next(),
                AccessTokenType = AccessTokenType.Reference,
                AllowAccessTokensViaBrowser = true,
                AllowRememberConsent = true,
                AllowedCorsOrigins = new List<ClientCorsOrigin> { new() { Origin = $"http://localhost:{random.Next()}" } },
                AllowedScopes = new List<ClientScope> { new() { Scope = "openid" }, new() { Scope = "profile" }, new() { Scope = "api1" } },
                AlwaysSendClientClaims = true,
                AuthorizationCodeLifetime = random.Next(),
                ClientId = Guid.NewGuid().ToString(),
                ClientName = Guid.NewGuid().ToString(),
                ClientUri = $"http://localhost:{random.Next()}",
                EnableLocalLogin = false,
                Enabled = false,
                IdentityProviderRestrictions = new List<ClientIdPRestriction> { new() { Provider = Guid.NewGuid().ToString() } },
                IdentityTokenLifetime = random.Next(),
                IncludeJwtId = true,
                LogoUri = $"http://localhost:{random.Next()}",
                LogoutSessionRequired = true,
                LogoutUri = $"http://localhost:{random.Next()}",
                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUri> { new() { Uri = $"http://localhost:{random.Next()}" } },
                PrefixClientClaims = true,
                RedirectUris = new List<ClientRedirectUri> { new() { Uri = $"http://localhost:{random.Next()}" } },
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.ReUse,
                RequireConsent = false,
                SlidingRefreshTokenLifetime = random.Next(),
                UpdateAccessTokenOnRefresh = true,
                AllowAccessToAllGrantTypes = true, // data will be lost
                AllowAccessToAllScopes = true, // data will be lost
                AllowClientCredentialsOnly = true, // data will be lost
                RequireSignOutPrompt = true // data will be lost
            };

            var mappedClient = client.ToDuende();

            mappedClient.Should().NotBeNull();
            mappedClient.AbsoluteRefreshTokenLifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
            mappedClient.AccessTokenLifetime.Should().Be(client.AccessTokenLifetime);
            mappedClient.AccessTokenType.Should().Be(Duende.IdentityServer.Models.AccessTokenType.Reference);
            mappedClient.AllowAccessTokensViaBrowser.Should().Be(client.AllowAccessTokensViaBrowser);
            mappedClient.AllowRememberConsent.Should().Be(client.AllowRememberConsent);
            mappedClient.AllowedCorsOrigins.Should().BeEquivalentTo(client.AllowedCorsOrigins.FirstOrDefault()?.Origin);
            mappedClient.AllowedScopes.Should().Contain("openid");
            mappedClient.AllowedScopes.Should().Contain("profile");
            mappedClient.AllowedScopes.Should().Contain("api1");
            mappedClient.AlwaysSendClientClaims.Should().Be(client.AlwaysSendClientClaims);
            mappedClient.AuthorizationCodeLifetime.Should().Be(client.AuthorizationCodeLifetime);
            mappedClient.ClientId.Should().Be(client.ClientId);
            mappedClient.ClientName.Should().Be(client.ClientName);
            mappedClient.ClientUri.Should().Be(client.ClientUri);
            mappedClient.EnableLocalLogin.Should().Be(client.EnableLocalLogin);
            mappedClient.Enabled.Should().Be(client.Enabled);
            mappedClient.FrontChannelLogoutUri.Should().Be(client.LogoutUri);
            mappedClient.FrontChannelLogoutSessionRequired.Should().Be(client.LogoutSessionRequired);
            mappedClient.IdentityProviderRestrictions.Should().BeEquivalentTo(client.IdentityProviderRestrictions.FirstOrDefault()?.Provider);
            mappedClient.IdentityTokenLifetime.Should().Be(client.IdentityTokenLifetime);
            mappedClient.IncludeJwtId.Should().Be(client.IncludeJwtId);
            mappedClient.LogoUri.Should().Be(client.LogoUri);
            mappedClient.PostLogoutRedirectUris.Should().BeEquivalentTo(client.PostLogoutRedirectUris.FirstOrDefault()?.Uri);
            mappedClient.RedirectUris.Should().BeEquivalentTo(client.RedirectUris.FirstOrDefault()?.Uri);
            mappedClient.RefreshTokenExpiration.Should().Be(Duende.IdentityServer.Models.TokenExpiration.Sliding);
            mappedClient.RefreshTokenUsage.Should().Be(Duende.IdentityServer.Models.TokenUsage.ReUse);
            mappedClient.RequireConsent.Should().Be(client.RequireConsent);
            mappedClient.SlidingRefreshTokenLifetime.Should().Be(client.SlidingRefreshTokenLifetime);
            mappedClient.UpdateAccessTokenClaimsOnRefresh.Should().Be(client.UpdateAccessTokenOnRefresh);

            if (client.PrefixClientClaims) mappedClient.ClientClaimsPrefix.Should().Be("client_");
            else mappedClient.ClientClaimsPrefix.Should().BeNull();
        }

        [Fact]
        public void GivenClientWithCustomGrantTypes_ExpectGrantTypesMappedCorrectly()
        {
            const string customGrant = "special_custom";
            var client = new Client { AllowedCustomGrantTypes = new List<ClientCustomGrantType> { new() { GrantType = customGrant } }, Flow = Flows.Custom };

            var mappedClient = client.ToDuende();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedGrantTypes.Should().HaveCount(client.AllowedCustomGrantTypes.Count);
            mappedClient.AllowedGrantTypes.Should().Contain(customGrant);
        }

        [Fact]
        public void GivenClientWithOfflineAccess_ExpectOfflineAccessMappedCorrectly()
        {
            const string offlineAccess = "offline_access";
            var client = new Client { AllowedScopes = new List<ClientScope> { new() { Scope = offlineAccess } } };

            var mappedClient = client.ToDuende();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedScopes.Should().NotContain(offlineAccess);
            mappedClient.AllowOfflineAccess.Should().BeTrue();
        }

        [Fact]
        public void GivenClientWithSecrets_ExpectSecretsMappedCorrectly()
        {
            var secret = new ClientSecret()
            {
                Type = Guid.NewGuid().ToString(),
                Value = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Expiration = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            var client = new Client { ClientSecrets = new List<ClientSecret> { secret } };

            var mappedClient = client.ToDuende();

            mappedClient.Should().NotBeNull();
            mappedClient.ClientSecrets.Should().NotBeNull();
            mappedClient.ClientSecrets.Should().NotBeEmpty();
            mappedClient.ClientSecrets.Should().HaveCount(client.ClientSecrets.Count);

            var mappedSecret = mappedClient.ClientSecrets.First();
            mappedSecret.Type.Should().Be(secret.Type);
            mappedSecret.Value.Should().Be(secret.Value);
            mappedSecret.Description.Should().Be(secret.Description);
            if (secret.Expiration != null)
                mappedSecret.Expiration?.Ticks.Should().Be(secret.Expiration.Value.Ticks);
        }

        [Fact]
        public void GivenClientWithPkceFlow_ExpectFlowAndPkceRequirementMappedCorrectly()
        {
            var client = new Client
            {
                Flow = Flows.AuthorizationCodeWithProofKey
            };

            var mappedClient = client.ToDuende();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedGrantTypes.Should().Contain(GrantType.AuthorizationCode);
            mappedClient.RequirePkce.Should().BeTrue();
            mappedClient.AllowPlainTextPkce.Should().BeFalse();
        }
    }
}