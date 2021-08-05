using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework;
using IdentityServer3.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore.Design;

namespace Rsk.IdentityServer.Migration.Readers
{
    public class EntityFrameworkScopeReader : IScopeReader
    {
        private readonly ScopeConfigurationDbContext context;

        public EntityFrameworkScopeReader(ScopeConfigurationDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<List<Scope>> Read()
        {
            var scopes = context.Scopes
                .Include(x => x.ScopeClaims)
                .Include(x=>x.ScopeSecrets)
                .ToListAsync();

            return scopes;
        }
    }
}