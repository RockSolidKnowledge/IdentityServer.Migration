using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer4;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;

namespace Rsk.IdentityServer.Migration.Tests.Mappers
{
    public class ApiResourceMapperTests
    {
        [Fact]
        public void GivenIdentityScope_ExpectEmptyCollectionReturned()
        {
            var scope = new Scope { Type = ScopeType.Identity };
            var scopes = new List<Scope> {scope};

            scopes.GetApiResources().Should().BeEmpty();

        }

        [Fact]
        public void GivenApiScope_ExpectApiResourceCorrectlyMapped()
        {
            var scope = new Scope
            {
                Description = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Emphasize = true,
                Enabled = false,
                Name = Guid.NewGuid().ToString(),
                Required = true,
                ShowInDiscoveryDocument = false,
                Type = ScopeType.Resource,

                AllowUnrestrictedIntrospection = true, // data will be lost
                ClaimsRule = Guid.NewGuid().ToString(), // data will be lost
                IncludeAllClaimsForUser = true, // data will be lost
            };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetApiResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.Should().NotBeNull();
            resource.Name.Should().Be(scope.Name);
            resource.DisplayName.Should().Be(scope.DisplayName);
            resource.Description.Should().Be(scope.Description);
            resource.Enabled.Should().Be(scope.Enabled);
        }

        [Fact]
        public void GivenApiScope_ExpectApiScopeCorrectlyMapped()
        {
            var scope = new Scope
            {
                Description = Guid.NewGuid().ToString(),
                DisplayName = Guid.NewGuid().ToString(),
                Emphasize = true,
                Name = Guid.NewGuid().ToString(),
                Required = true,
                ShowInDiscoveryDocument = false,
                Type = ScopeType.Resource
            };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetApiResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.Should().NotBeNull();
            resource.Scopes.Should().NotBeEmpty();
            resource.Scopes.Should().HaveCount(1);

            var resourceScope = resource.Scopes.Single();
            resourceScope.Description.Should().Be(scope.Description);
            resourceScope.DisplayName.Should().Be(scope.DisplayName);
            resourceScope.Emphasize.Should().Be(scope.Emphasize);
            resourceScope.Name.Should().Be(scope.Name);
            resourceScope.Required.Should().Be(scope.Required);
            resourceScope.ShowInDiscoveryDocument.Should().Be(scope.ShowInDiscoveryDocument);
            resourceScope.UserClaims.Should().BeEmpty();
        }

        [Fact]
        public void GivenApiScopeWithUserClaims_ExpectClaimsCorrectlyMapped()
        {
            var scopeClaim = new ScopeClaim
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(), // data will be lost
                AlwaysIncludeInIdToken = true // data will be lost
            };
            var scope = new Scope { Type = ScopeType.Resource, Claims = { scopeClaim } };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetApiResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.UserClaims.Should().NotBeEmpty();
            resource.UserClaims.Should().HaveCount(scope.Claims.Count);
            resource.UserClaims.Should().Contain(scopeClaim.Name);

            resource.Scopes.Should().NotBeEmpty();
            resource.Scopes.Should().HaveCount(1);
            var resourceScope = resource.Scopes.Single();
            resourceScope.UserClaims.Should().BeEmpty();
        }

        [Fact]
        public void GivenApiScopeWithSecrets_ExpectSecretsCorrectlyMapped()
        {
            var secret = new Secret
            {
                Type = Constants.SecretTypes.SharedSecret,
                Value = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Expiration = DateTimeOffset.UtcNow.AddDays(2)
            };
            var scope = new Scope { Type = ScopeType.Resource, ScopeSecrets = { secret } };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetApiResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.ApiSecrets.Should().NotBeEmpty();
            resource.ApiSecrets.Should().HaveCount(scope.ScopeSecrets.Count);

            var apiSecret = resource.ApiSecrets.First();
            apiSecret.Type.Should().BeEquivalentTo(secret.Type);
            apiSecret.Type.Should().BeEquivalentTo(IdentityServerConstants.SecretTypes.SharedSecret);
            apiSecret.Value.Should().BeEquivalentTo(secret.Value);
            apiSecret.Description.Should().BeEquivalentTo(secret.Description);
            apiSecret.Expiration?.Should().BeExactly(new TimeSpan(secret.Expiration.Value.Ticks));
        }
    }
}