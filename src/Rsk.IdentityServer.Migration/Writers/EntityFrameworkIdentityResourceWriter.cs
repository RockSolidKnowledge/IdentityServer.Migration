using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;

namespace Rsk.IdentityServer.Migration.Writers
{
    public class EntityFrameworkIdentityResourceWriter : IIdentityResourceWriter
    {
        private readonly ConfigurationDbContext context;

        public EntityFrameworkIdentityResourceWriter(ConfigurationDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(IEnumerable<IdentityResource> resources)
        {
            await context.IdentityResources.AddRangeAsync(resources.Select(x => x.ToEntity()).ToList());
            await context.SaveChangesAsync();
        }
    }
}