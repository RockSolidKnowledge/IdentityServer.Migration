using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework.Entities;
using Rsk.IdentityServer.Migration.Mappers;
using Xunit;
using Scope = IdentityServer3.EntityFramework.Entities.Scope;
using ScopeClaim = IdentityServer3.EntityFramework.Entities.ScopeClaim;

namespace Rsk.IdentityServer.Migration.Tests.Mappers
{
    public class IdentityResourceMapperTests
    {
        [Fact]
        public void GivenApiScope_ExpectEmptyCollectionReturned()
        {
            var scope = new Scope { Type = (int) ScopeType.Resource };
            var scopes = new List<Scope> {scope};

            scopes.GetIdentityResources().Should().BeEmpty();
        }

        [Fact]
        public void GivenIdentityScope_ExpectIdentityResourceCorrectlyMapped()
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
                Type = (int) ScopeType.Identity,
                AllowUnrestrictedIntrospection = true, // data will be lost
                ClaimsRule = Guid.NewGuid().ToString(), // data will be lost
                IncludeAllClaimsForUser = true, // data will be lost
                ScopeSecrets = new List<ScopeSecret>{ new() {Value = "secret"} } // data will be lost
            };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetIdentityResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.Should().NotBeNull();
            resource.Name.Should().Be(scope.Name);
            resource.DisplayName.Should().Be(scope.DisplayName);
            resource.Description.Should().Be(scope.Description);
            resource.Emphasize.Should().Be(scope.Emphasize);
            resource.Enabled.Should().Be(scope.Enabled);
            resource.Required.Should().Be(scope.Required);
            resource.ShowInDiscoveryDocument.Should().Be(scope.ShowInDiscoveryDocument);
        }

        [Fact]
        public void GivenIds3IdentityScopeWithUserClaims_ExpectClaimsCorrectlyMapped()
        {
            var scopeClaim = new ScopeClaim
            {
                Name = Guid.NewGuid().ToString(),
                Description = Guid.NewGuid().ToString(), // data will be lost
                AlwaysIncludeInIdToken = true // data will be lost
            };
            var scope = new Scope { Type = (int) ScopeType.Identity, ScopeClaims= new List<ScopeClaim>(){ scopeClaim } };
            var scopes = new List<Scope> { scope };

            var resources = scopes.GetIdentityResources();

            resources.Should().NotBeEmpty();
            resources.Should().HaveCount(scopes.Count);

            var resource = resources.Single(x => x.Name == scope.Name);

            resource.Should().NotBeNull();
            resource.UserClaims.Should().NotBeEmpty();
            resource.UserClaims.Should().Contain(scopeClaim.Name);
        }
    }
}
