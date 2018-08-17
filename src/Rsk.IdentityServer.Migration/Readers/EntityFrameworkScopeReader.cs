using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using Microsoft.Extensions.Options;
using EntitiesMap = IdentityServer3.EntityFramework.Entities.EntitiesMap;

namespace Rsk.IdentityServer.Migration.Readers
{
    public class EntityFrameworkScopeReader : IScopeReader
    {
        private readonly DbOptions options;

        public EntityFrameworkScopeReader(IOptions<DbOptions> optionsAccessor)
        {
            options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        public async Task<IList<Scope>> Read()
        {
            using (var context = new ScopeConfigurationDbContext(options.IdentityServer3ConnectionString))
            {
                var scopes = await context.Scopes.ToListAsync();
                return scopes.Select(EntitiesMap.ToModel).ToList();
            }
        }
    }
}