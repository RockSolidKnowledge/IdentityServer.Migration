using System;
using System.Linq;
using System.Threading.Tasks;
using Rsk.IdentityServer.Migration.Mappers;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;
using Rsk.IdentityServer.Migration.Writers.Interfaces;

namespace Rsk.IdentityServer.Migration
{
    public class Migrator
    {
        private readonly IClientReader clientReader;
        private readonly IScopeReader scopeReader;
        private readonly IClientWriter clientWriter;
        private readonly ITokenReader tokenReader;
        private readonly IApiResourceWriter apiResourceWriter;
        private readonly IIdentityResourceWriter identityResourceWriter;
        private readonly IPersistedGrantsWriter persistedGrantsWriter;

        public Migrator(
            IClientReader clientReader,
            IScopeReader scopeReader,
            ITokenReader tokenReader,
            IClientWriter clientWriter,
            IApiResourceWriter apiResourceWriter,
            IIdentityResourceWriter identityResourceWriter,
            IPersistedGrantsWriter persistedGrantsWriter)
        {
            this.clientReader = clientReader ?? throw new ArgumentNullException(nameof(clientReader));
            this.scopeReader = scopeReader ?? throw new ArgumentNullException(nameof(scopeReader));
            this.tokenReader = tokenReader ?? throw new ArgumentNullException(nameof(tokenReader));
            this.clientWriter = clientWriter ?? throw new ArgumentNullException(nameof(clientWriter));
            this.apiResourceWriter = apiResourceWriter ?? throw new ArgumentNullException(nameof(apiResourceWriter));
            this.identityResourceWriter = identityResourceWriter ?? throw new ArgumentNullException(nameof(identityResourceWriter));
            this.persistedGrantsWriter = persistedGrantsWriter ?? throw new ArgumentNullException(nameof(persistedGrantsWriter));
        }

        public async Task Migrate()
        {
            var clients = await clientReader.Read();
            var scopes = await scopeReader.Read();
            var (tokens, consents) = await tokenReader.Read();
           
            await clientWriter.Write(clients.Select(x => x.ToVersion4()).ToList());

            var result = scopes.GetApiResourcesAndApiScopes();
            await apiResourceWriter.Write(result.apiResources, result.scopes);

            await identityResourceWriter.Write(scopes.GetIdentityResources());

            await persistedGrantsWriter.Write(tokens, consents);
        }
    }
}