using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework;
using IdentityServer3.EntityFramework.Entities;

namespace Rsk.IdentityServer.Migration.Readers
{
    public class EntityFrameworkClientReader : IClientReader
    {
        private readonly ClientConfigurationDbContext context;

        public EntityFrameworkClientReader(ClientConfigurationDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<Client>> Read()
        {
            var clients = context.Clients
                .Include(x => x.AllowedCorsOrigins)
                .Include(x => x.AllowedCustomGrantTypes)
                .Include(x => x.AllowedScopes)
                .Include(x => x.Claims)
                .Include(x => x.IdentityProviderRestrictions)
                .Include(x => x.RedirectUris)
                .Include(x => x.PostLogoutRedirectUris)
                .Include(x => x.ClientSecrets)
                .ToListAsync();

            return clients;
        }
    }
}