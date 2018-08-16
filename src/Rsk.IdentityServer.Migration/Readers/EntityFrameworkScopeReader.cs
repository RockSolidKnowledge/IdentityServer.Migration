using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core.Models;
using IdentityServer3.EntityFramework;
using Rsk.IdentityServer.Migration.Readers.Interfaces;
using EntitiesMap = IdentityServer3.EntityFramework.Entities.EntitiesMap;

namespace Rsk.IdentityServer.Migration.Readers
{
    public class EntityFrameworkScopeReader : IScopeReader
    {
        public async Task<IList<Scope>> Read()
        {
            using (var context = new ScopeConfigurationDbContext())
            {
                var scopes = await context.Scopes.ToListAsync();
                return scopes.Select(EntitiesMap.ToModel).ToList();
            }
        }
    }
}