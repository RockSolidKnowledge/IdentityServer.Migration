using System.Collections.Generic;
using System.Linq;
using IdentityServer3.Core.Models;
using IdentityServer4.Models;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class IdentityResourceMappers
    {
        public static List<IdentityResource> GetIdentityResources(this IEnumerable<IdentityServer3.Core.Models.Scope> scopes) =>
            scopes.Where(x => x.Type == ScopeType.Identity).Select(x => x.ToVersion4()).ToList();

        private static IdentityResource ToVersion4(this IdentityServer3.Core.Models.Scope scope)
        {
            if (scope == null) return null;
            if (scope.Type != ScopeType.Identity) return null;

            return new IdentityResource
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Enabled = scope.Enabled,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                UserClaims = scope.Claims.Select(x => x.Name).ToList()
            };
        }
    }
}