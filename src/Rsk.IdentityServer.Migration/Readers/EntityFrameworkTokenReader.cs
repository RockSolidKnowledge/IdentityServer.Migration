using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework;
using Microsoft.Extensions.Options;
using Consent = IdentityServer3.EntityFramework.Entities.Consent;
using Token = IdentityServer3.EntityFramework.Entities.Token;

namespace Rsk.IdentityServer.Migration.Readers
{
    public class EntityFrameworkTokenReader : ITokenReader
    {
        private readonly DbOptions options;

        public EntityFrameworkTokenReader(IOptions<DbOptions> options)
        {
            this.options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<(IList<Token>, IList<Consent>)> Read()
        {
            using (var context = new OperationalDbContext(options.IdentityServer3OperationalConnectionString))
            {
                var tokens = await context.Tokens.ToListAsync();
                var consents = await context.Consents.ToListAsync();

                return (tokens, consents);
            }
        }
    }
}
