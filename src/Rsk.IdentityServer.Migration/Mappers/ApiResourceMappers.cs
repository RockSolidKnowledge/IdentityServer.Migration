using System.Collections.Generic;
using System.Linq;
using IdentityServer3.Core.Models;
using IdentityServer4.Models;
using Scope = IdentityServer4.Models.Scope;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class ApiResourceMappers
    {
        public static List<ApiResource> GetApiResources(this IEnumerable<IdentityServer3.Core.Models.Scope> scopes) =>
            scopes.Where(x => x.Type == ScopeType.Resource && x.Name != "offline_access").Select(x => x.ToVersion4()).ToList();
        
        private static ApiResource ToVersion4(this IdentityServer3.Core.Models.Scope scope)
        {
            if (scope == null) return null;
            if (scope.Type != ScopeType.Resource) return null;

            return new ApiResource
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Enabled = scope.Enabled,
                Scopes =
                {
                    new Scope
                    {
                        Name = scope.Name,
                        DisplayName = scope.DisplayName,
                        Description = scope.Description,
                        Emphasize = scope.Emphasize,
                        Required = scope.Required,
                        ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument
                    }
                },
                ApiSecrets = scope.ScopeSecrets.Select(x => x.ToVersion4()).ToList(),
                UserClaims = scope.Claims.Select(x => x.Name).ToList()
            };
        }
    }
}
