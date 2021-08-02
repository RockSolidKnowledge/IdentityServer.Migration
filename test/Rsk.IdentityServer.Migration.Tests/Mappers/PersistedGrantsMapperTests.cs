using System;
using System.Collections.Generic;
using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Stores.Serialization;
using FluentAssertions;
using IdentityServer3.Core.Models;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;
using Consent = IdentityServer3.EntityFramework.Entities.Consent;

namespace Rsk.IdentityServer.Migration.Tests.Mappers
{
    public class PersistedGrantsMapperTests
    {
        [Fact]
        public void ToV4Consent_WhenCalled_ShouldMapConsentAsExpected()
        {
            var ids3Consent = new Consent
            {
                ClientId = "client-id",
                Scopes = "openid,profile,api1",
                Subject = "subject"
            };

            var ids4Consent = ids3Consent.ToV4Consent();

            ids4Consent.ClientId.Should().Be(ids3Consent.ClientId);
            ids4Consent.SubjectId.Should().Be(ids3Consent.Subject);
            ids4Consent.Scopes.Should().BeEquivalentTo(ids3Consent.Scopes.Split(','));
            ids4Consent.Expiration.Should().BeNull();
        }

        [Fact]
        public void ToVersion4RefreshToken_WhenCalled_ShouldMapRefreshTokenAsExpected()
        {
            var ids3RefreshToken = new RefreshToken
            {
                AccessToken = new Token
                {
                    CreationTime = new DateTimeOffset(new DateTime(2019, 01, 01)),
                    Type = "access_token",
                    Claims = new List<Claim> { new Claim("type", "value") },
                    Issuer = "issuer",
                    Lifetime = 10,
                    Version = 3,
                    Audience = "audience",
                    Client = new Client{ClientId = "client-id"}
                },
                CreationTime = new DateTimeOffset(new DateTime(2019, 01, 01)),
                LifeTime = 10,
                Version = 3
            };

            var ids4RefreshToken = ids3RefreshToken.ToVersion4RefreshToken();

            ids4RefreshToken.CreationTime.Should().Be(ids3RefreshToken.CreationTime.UtcDateTime);
            ids4RefreshToken.Lifetime.Should().Be(ids3RefreshToken.LifeTime);
            ids4RefreshToken.Version.Should().Be(ids3RefreshToken.Version);

            ids4RefreshToken.AccessToken.CreationTime.Should().Be(ids3RefreshToken.AccessToken.CreationTime.UtcDateTime);
            ids4RefreshToken.AccessToken.ClientId.Should().Be(ids3RefreshToken.AccessToken.ClientId);
            ids4RefreshToken.AccessToken.Audiences.Should().Contain(ids3RefreshToken.AccessToken.Audience);
            ids4RefreshToken.AccessToken.Type.Should().Be(ids3RefreshToken.AccessToken.Type);
            ids4RefreshToken.AccessToken.Version.Should().Be(ids3RefreshToken.AccessToken.Version);
            ids4RefreshToken.AccessToken.Lifetime.Should().Be(ids3RefreshToken.AccessToken.Lifetime);
            ids4RefreshToken.AccessToken.Issuer.Should().Be(ids3RefreshToken.AccessToken.Issuer);
            ids4RefreshToken.AccessToken.Claims.Should().Contain(ids3RefreshToken.AccessToken.Claims);
        }

        [Fact]
        public void ToV4RefreshTokenAsPersistedGrant_WhenCalled_ShouldMapRefreshTokenAsExpected()
        {
            var serializer = new PersistentGrantSerializer();

            var ids3RefreshToken = new RefreshToken
            {
                AccessToken = new Token
                {
                    CreationTime = new DateTimeOffset(new DateTime(2019, 01, 01)),
                    Type = "access_token",
                    Claims = new List<Claim> { new Claim("type", "value") },
                    Issuer = "issuer",
                    Lifetime = 10,
                    Version = 3,
                    Audience = "audience",
                    Client = new Client{ClientId = "client-id"}
                },
                CreationTime = new DateTimeOffset(new DateTime(2019, 01, 01)),
                LifeTime = 10,
                Version = 3
            };

            var refreshTokenJson = serializer.Serialize(ids3RefreshToken);
            
            var token = new IdentityServer3.EntityFramework.Entities.Token
            {
                Key = "key",
                JsonCode = refreshTokenJson
            };

            var result = token.ToV4RefreshTokenAsPersistedGrant(serializer);

            result.Key.Should().Be(token.Key);
            result.ClientId.Should().Be(ids3RefreshToken.ClientId);
            result.CreationTime.Should().Be(ids3RefreshToken.CreationTime.UtcDateTime);
            result.Expiration.Should().Be(ids3RefreshToken.CreationTime.AddSeconds(ids3RefreshToken.LifeTime).UtcDateTime);
            result.SubjectId.Should().Be(ids3RefreshToken.SubjectId);
            result.Type.Should().Be(IdentityServerConstants.PersistedGrantTypes.RefreshToken);
        }

        [Fact]
        public void ToVersion4ReferenceToken_WhenCalled_ShouldMapRefreshTokenAsExpected()
        {
            var serializer = new PersistentGrantSerializer();

            var ids3ReferenceToken = new IdentityServer3.Core.Models.Token
            {
                Client = new Client{ClientId = "client-id"},
                CreationTime = new DateTimeOffset(new DateTime(2019,01,01)),
                Audience = "audience",
                Lifetime = 100,
                Claims = new List<Claim>{new Claim("type","value")},
                Issuer = "issuer",
                Version = 3
            };

            var referenceTokenJson = serializer.Serialize(ids3ReferenceToken);
            
            var token = new IdentityServer3.EntityFramework.Entities.Token
            {
                Key = "key",
                JsonCode = referenceTokenJson
            };

            var result = token.ToVersion4ReferenceToken(serializer);

            result.Key.Should().Be(token.Key);
            result.ClientId.Should().Be(ids3ReferenceToken.ClientId);
            result.CreationTime.Should().Be(ids3ReferenceToken.CreationTime.UtcDateTime);
            result.Expiration.Should().Be(ids3ReferenceToken.CreationTime.AddSeconds(ids3ReferenceToken.Lifetime).UtcDateTime);
            result.SubjectId.Should().Be(ids3ReferenceToken.SubjectId);
            result.Type.Should().Be(IdentityServerConstants.PersistedGrantTypes.ReferenceToken);
        }
    }
}
