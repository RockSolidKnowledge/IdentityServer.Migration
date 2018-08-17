using System;
using System.Linq;
using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer4.Models;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;
using AccessTokenType = IdentityServer3.Core.Models.AccessTokenType;
using Client = IdentityServer3.Core.Models.Client;
using Secret = IdentityServer3.Core.Models.Secret;
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
                AllowedCorsOrigins = { $"http://localhost:{random.Next()}" },
                AllowedScopes = { "openid", "profile", "api1" },
                AlwaysSendClientClaims = true,
                AuthorizationCodeLifetime = random.Next(),
                ClientId = Guid.NewGuid().ToString(),
                ClientName = Guid.NewGuid().ToString(),
                ClientUri = $"http://localhost:{random.Next()}",
                EnableLocalLogin = false,
                Enabled = false,
                IdentityProviderRestrictions = { Guid.NewGuid().ToString() },
                IdentityTokenLifetime = random.Next(),
                IncludeJwtId = true,
                LogoUri = $"http://localhost:{random.Next()}",
                LogoutSessionRequired = true,
                LogoutUri = $"http://localhost:{random.Next()}",
                PostLogoutRedirectUris = { $"http://localhost:{random.Next()}" },
                PrefixClientClaims = true,
                RedirectUris = { $"http://localhost:{random.Next()}" },
                RefreshTokenExpiration = TokenExpiration.Sliding,
                RefreshTokenUsage = TokenUsage.ReUse,
                RequireConsent = false,
                SlidingRefreshTokenLifetime = random.Next(),
                UpdateAccessTokenClaimsOnRefresh = true,
                AllowAccessToAllCustomGrantTypes = true, // data will be lost
                AllowAccessToAllScopes = true, // data will be lost
                AllowClientCredentialsOnly = true, // data will be lost
                RequireSignOutPrompt = true // data will be lost
            };

            var mappedClient = client.ToVersion4();

            mappedClient.Should().NotBeNull();
            mappedClient.AbsoluteRefreshTokenLifetime.Should().Be(client.AbsoluteRefreshTokenLifetime);
            mappedClient.AccessTokenLifetime.Should().Be(client.AccessTokenLifetime);
            mappedClient.AccessTokenType.Should().Be(IdentityServer4.Models.AccessTokenType.Reference);
            mappedClient.AllowAccessTokensViaBrowser.Should().Be(client.AllowAccessTokensViaBrowser);
            mappedClient.AllowRememberConsent.Should().Be(client.AllowRememberConsent);
            mappedClient.AllowedCorsOrigins.Should().BeEquivalentTo(client.AllowedCorsOrigins);
            mappedClient.AllowedScopes.Should().BeEquivalentTo(client.AllowedScopes);
            mappedClient.AlwaysSendClientClaims.Should().Be(client.AlwaysSendClientClaims);
            mappedClient.AuthorizationCodeLifetime.Should().Be(client.AuthorizationCodeLifetime);
            mappedClient.ClientId.Should().Be(client.ClientId);
            mappedClient.ClientName.Should().Be(client.ClientName);
            mappedClient.ClientUri.Should().Be(client.ClientUri);
            mappedClient.EnableLocalLogin.Should().Be(client.EnableLocalLogin);
            mappedClient.Enabled.Should().Be(client.Enabled);
            mappedClient.FrontChannelLogoutUri.Should().Be(client.LogoutUri);
            mappedClient.FrontChannelLogoutSessionRequired.Should().Be(client.LogoutSessionRequired);
            mappedClient.IdentityProviderRestrictions.Should().BeEquivalentTo(client.IdentityProviderRestrictions);
            mappedClient.IdentityTokenLifetime.Should().Be(client.IdentityTokenLifetime);
            mappedClient.IncludeJwtId.Should().Be(client.IncludeJwtId);
            mappedClient.LogoUri.Should().Be(client.LogoUri);
            mappedClient.PostLogoutRedirectUris.Should().BeEquivalentTo(client.PostLogoutRedirectUris);
            mappedClient.RedirectUris.Should().BeEquivalentTo(client.RedirectUris);
            mappedClient.RefreshTokenExpiration.Should().Be(IdentityServer4.Models.TokenExpiration.Sliding);
            mappedClient.RefreshTokenUsage.Should().Be(IdentityServer4.Models.TokenUsage.ReUse);
            mappedClient.RequireConsent.Should().Be(client.RequireConsent);
            mappedClient.SlidingRefreshTokenLifetime.Should().Be(client.SlidingRefreshTokenLifetime);
            mappedClient.UpdateAccessTokenClaimsOnRefresh.Should().Be(client.UpdateAccessTokenClaimsOnRefresh);

            if (client.PrefixClientClaims) mappedClient.ClientClaimsPrefix.Should().Be("client_");
            else mappedClient.ClientClaimsPrefix.Should().BeNull();
        }

        [Fact]
        public void GivenClientWithCustomGrantTypes_ExpectGrantTypesMappedCorrectly()
        {
            const string customGrant = "special_custom";
            var client = new Client { AllowedCustomGrantTypes = { customGrant }, Flow = Flows.Custom };

            var mappedClient = client.ToVersion4();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedGrantTypes.Should().HaveCount(client.AllowedCustomGrantTypes.Count);
            mappedClient.AllowedGrantTypes.Should().Contain(customGrant);
        }

        [Fact]
        public void GivenClientWithOfflineAccess_ExpectOfflineAccessMappedCorrectly()
        {
            const string offlineAccess = "offline_access";
            var client = new Client { AllowedScopes = { offlineAccess } };

            var mappedClient = client.ToVersion4();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedScopes.Should().NotContain(offlineAccess);
            mappedClient.AllowOfflineAccess.Should().BeTrue();
        }

        [Fact]
        public void GivenClientWithSecrets_ExpectSecretsMappedCorrectly()
        {
            var secret = new Secret
            {
                Type = Guid.NewGuid().ToString(),
                Value = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Expiration = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            var client = new Client { ClientSecrets = { secret } };

            var mappedClient = client.ToVersion4();

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

            var mappedClient = client.ToVersion4();

            mappedClient.Should().NotBeNull();
            mappedClient.AllowedGrantTypes.Should().Contain(GrantType.AuthorizationCode);
            mappedClient.RequirePkce.Should().BeTrue();
            mappedClient.AllowPlainTextPkce.Should().BeFalse();
        }
    }
}