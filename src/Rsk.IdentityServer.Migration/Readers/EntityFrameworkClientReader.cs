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
    public class EntityFrameworkClientReader : IClientReader
    {
        private readonly DbOptions options;

        public EntityFrameworkClientReader(IOptions<DbOptions> optionsAccessor)
        {
            options = optionsAccessor?.Value ?? throw new ArgumentNullException(nameof(optionsAccessor));
        }

        public async Task<IList<Client>> Read()
        {
            using (var context = new ClientConfigurationDbContext(options.IdentityServer3ConnectionString))
            {
                var clients = await context.Clients.ToListAsync();
                return clients.Select(EntitiesMap.ToModel).ToList();
            }
        }
    }
}