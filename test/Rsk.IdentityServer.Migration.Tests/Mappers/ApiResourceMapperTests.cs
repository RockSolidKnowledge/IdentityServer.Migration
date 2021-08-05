using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer;
using FluentAssertions;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework.Entities;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;
using Scope = IdentityServer3.EntityFramework.Entities.Scope;
using ScopeClaim = IdentityServer3.EntityFramework.Entities.ScopeClaim;

namespace Rsk.IdentityServer.Migration.Tests.Mappers
{
    public class ApiResourceMapperTests
    {
        [Fact]
        public void GivenIdentityScope_ExpectEmptyCollectionReturned()
        {
            var scope = new Scope { Type = (int) ScopeType.Identity };
            var scopes = new List<Scope> {scope};

            var result = scopes.GetApiResourcesAndApiScopes();
            result.apiResources.Should().BeEmpty();
            result.scopes.Should().BeEmpty();
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
                Type = (int) ScopeType.Resource,

                AllowUnrestrictedIntrospection = true, // data will be lost
                ClaimsRule = Guid.NewGuid().ToString(), // data will be lost
                IncludeAllClaimsForUser = true, // data will be lost
            };
            var scopes = new List<Scope> { scope };

            var result = scopes.GetApiResourcesAndApiScopes();

            result.apiResources.Should().NotBeEmpty();
            result.scopes.Should().NotBeEmpty();

            result.apiResources.Should().HaveCount(scopes.Count);
            result.scopes.Should().HaveCount(scopes.Count);
            
            var resource = result.apiResources.Single(x => x.Name == scope.Name);

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
                Type = (int) ScopeType.Resource
            };
            var scopes = new List<Scope> { scope };

            var result = scopes.GetApiResourcesAndApiScopes();

            result.apiResources.Should().NotBeEmpty();
            result.scopes.Should().NotBeEmpty();

            result.apiResources.Should().HaveCount(scopes.Count);
            result.scopes.Should().HaveCount(scopes.Count);

            var resource = result.apiResources.Single(x => x.Name == scope.Name);

            resource.Should().NotBeNull();
            resource.Scopes.Should().NotBeEmpty();
            resource.Scopes.Should().HaveCount(1);

            var apiScope = result.scopes.Single();
            apiScope.Description.Should().Be(scope.Description);
            apiScope.DisplayName.Should().Be(scope.DisplayName);
            apiScope.Emphasize.Should().Be(scope.Emphasize);
            apiScope.Name.Should().Be(scope.Name);
            apiScope.Required.Should().Be(scope.Required);
            apiScope.ShowInDiscoveryDocument.Should().Be(scope.ShowInDiscoveryDocument);
            apiScope.UserClaims.Should().BeEmpty();
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
            var scope = new Scope { Type = (int) ScopeType.Resource, ScopeClaims = new List<ScopeClaim>{ scopeClaim } };
            var scopes = new List<Scope> { scope };

            var result = scopes.GetApiResourcesAndApiScopes();
            
            result.apiResources.Should().NotBeEmpty();
            result.scopes.Should().NotBeEmpty();

            result.apiResources.Should().HaveCount(scopes.Count);
            result.scopes.Should().HaveCount(scopes.Count);

            var resource = result.apiResources.Single(x => x.Name == scope.Name);

            resource.UserClaims.Should().NotBeEmpty();
            resource.UserClaims.Should().HaveCount(scope.ScopeClaims.Count);
            resource.UserClaims.Should().Contain(scopeClaim.Name);

            resource.Scopes.Should().NotBeEmpty();
            resource.Scopes.Should().HaveCount(1);
            
            var apiScope = result.scopes.Single();
            apiScope.UserClaims.Should().BeEmpty();
        }

        [Fact]
        public void GivenApiScopeWithSecrets_ExpectSecretsCorrectlyMapped()
        {
            var secret = new ScopeSecret()
            {
                Type = Constants.SecretTypes.SharedSecret,
                Value = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(),
                Expiration = DateTimeOffset.UtcNow.AddDays(2)
            };

            var scope = new Scope { Type = (int) ScopeType.Resource, ScopeSecrets = new List<ScopeSecret> { secret } };
            var scopes = new List<Scope> { scope };

            var result = scopes.GetApiResourcesAndApiScopes();
            
            result.apiResources.Should().NotBeEmpty();
            result.scopes.Should().NotBeEmpty();

            result.apiResources.Should().HaveCount(scopes.Count);
            result.scopes.Should().HaveCount(scopes.Count);

            var resource = result.apiResources.Single(x => x.Name == scope.Name);

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