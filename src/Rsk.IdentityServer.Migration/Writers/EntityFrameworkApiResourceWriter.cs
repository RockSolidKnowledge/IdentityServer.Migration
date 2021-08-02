using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;

namespace Rsk.IdentityServer.Migration.Writers
{
    public class EntityFrameworkApiResourceWriter : IApiResourceWriter
    {
        private readonly ConfigurationDbContext context;

        public EntityFrameworkApiResourceWriter(ConfigurationDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task Write(IEnumerable<ApiResource> resources, IEnumerable<ApiScope> scopes)
        {
            await context.ApiScopes.AddRangeAsync(scopes.Select(x => x.ToEntity()));
            await context.ApiResources.AddRangeAsync(resources.Select(x => x.ToEntity()));
            await context.SaveChangesAsync();
        }
    }
}