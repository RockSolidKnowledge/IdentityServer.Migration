using System;
using System.Linq;
using System.Threading.Tasks;
using Rsk.IdentityServer.Migration.Mappers;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;

namespace Rsk.IdentityServer.Migration
{
    public class Migrator
    {
        private readonly IClientReader clientReader;
        private readonly IScopeReader scopeReader;
        private readonly IClientWriter clientWriter;
        private readonly IApiResourceWriter apiResourceWriter;
        private readonly IIdentityResourceWriter identityResourceWriter;

        public Migrator(
            IClientReader clientReader,
            IScopeReader scopeReader,
            IClientWriter clientWriter,
            IApiResourceWriter apiResourceWriter,
            IIdentityResourceWriter identityResourceWriter)
        {
            this.clientReader = clientReader ?? throw new ArgumentNullException(nameof(clientReader));
            this.scopeReader = scopeReader ?? throw new ArgumentNullException(nameof(scopeReader));
            this.clientWriter = clientWriter ?? throw new ArgumentNullException(nameof(clientWriter));
            this.apiResourceWriter = apiResourceWriter ?? throw new ArgumentNullException(nameof(apiResourceWriter));
            this.identityResourceWriter = identityResourceWriter ?? throw new ArgumentNullException(nameof(identityResourceWriter));
        }

        public async Task Migrate()
        {
            var clients = await clientReader.Read();
            var scopes = await scopeReader.Read();

            await clientWriter.Write(clients.Select(x => x.ToVersion4()).ToList());
            await apiResourceWriter.Write(scopes.GetApiResources());
            await identityResourceWriter.Write(scopes.GetIdentityResources());
        }
    }
}