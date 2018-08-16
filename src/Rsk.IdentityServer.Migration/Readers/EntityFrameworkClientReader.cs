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
    public class EntityFrameworkClientReader : IClientReader
    {
        public async Task<IList<Client>> Read()
        {
            using (var context = new ClientConfigurationDbContext())
            {
                var clients = await context.Clients.ToListAsync();
                return clients.Select(EntitiesMap.ToModel).ToList();
            }
        }
    }
}