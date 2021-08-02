using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer.Models;
using IdentityServer3.Core.Models;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class ApiResourceMappers
    {
        public static (List<ApiResource> apiResources, List<ApiScope> scopes) GetApiResourcesAndApiScopes(this IEnumerable<IdentityServer3.Core.Models.Scope> ids3Scopes)
        {
            var apiResources = new List<ApiResource>();
            var scopes = new List<ApiScope>();

            foreach (var ids3Scope in ids3Scopes)
            {
                if (ids3Scope.Type == ScopeType.Resource && ids3Scope.Name != "offline_access")
                {
                    var result = ids3Scope.ToDuende();

                    apiResources.Add(result.apiResource);
                    scopes.Add(result.scope);
                }
            }

            return (apiResources, scopes);
        }

        private static (ApiResource apiResource, ApiScope scope) ToDuende(this IdentityServer3.Core.Models.Scope ids3Scope)
        {
            if (ids3Scope == null) return (null, null);
            if (ids3Scope.Type != ScopeType.Resource) return (null, null);

            var scope = new ApiScope
            {
                Name = ids3Scope.Name,
                DisplayName = ids3Scope.DisplayName,
                Description = ids3Scope.Description,
                Emphasize = ids3Scope.Emphasize,
                Required = ids3Scope.Required,
                ShowInDiscoveryDocument = ids3Scope.ShowInDiscoveryDocument
            };

            var apiResource = new ApiResource
            {
                Name = ids3Scope.Name,
                DisplayName = ids3Scope.DisplayName,
                Description = ids3Scope.Description,
                Enabled = ids3Scope.Enabled,
                Scopes = new List<string> { ids3Scope.Name },
                ApiSecrets = ids3Scope.ScopeSecrets.Select(x => x.ToDuende()).ToList(),
                UserClaims = ids3Scope.Claims.Select(x => x.Name).ToList()
            };

            return (apiResource, scope);
        }
    }
}
