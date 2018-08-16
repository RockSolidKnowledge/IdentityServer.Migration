using System.Linq;
using IdentityServer3.Core.Models;
using IdentityServer4.Models;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class IdentityResourceMappers
    {
        public static IdentityResource ToVersion4(this IdentityServer3.Core.Models.Scope scope)
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