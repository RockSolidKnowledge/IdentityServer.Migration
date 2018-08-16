using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Rsk.IdentityServer.Migration.Writers.Interfaces;

namespace Rsk.IdentityServer.Migration.Writers
{
    public class EntityFrameworkApiResourceWriter : IApiResourceWriter
    {
        private readonly ConfigurationDbContext context;

        public EntityFrameworkApiResourceWriter(ConfigurationDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task Write(IEnumerable<ApiResource> resources)
        {
            await context.ApiResources.AddRangeAsync(resources.Select(x => x.ToEntity()).ToList());
            await context.SaveChangesAsync();
        }
    }
}