using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer3.EntityFramework.Entities;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Rsk.IdentityServer.Migration.Mappers;
using Rsk.IdentityServer.Migration.Writers.Interfaces;
using Consent = IdentityServer3.EntityFramework.Entities.Consent;
using Token = IdentityServer3.EntityFramework.Entities.Token;

namespace Rsk.IdentityServer.Migration.Writers
{
    public class EntityFrameworkPersistedGrantsWriter : IPersistedGrantsWriter
    {
        private readonly PersistedGrantDbContext context;
        private readonly PersistedGrantStore persistedGrantStore;
        private readonly PersistentGrantSerializer serializer;
        
        public EntityFrameworkPersistedGrantsWriter(PersistedGrantDbContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            persistedGrantStore = new PersistedGrantStore(context, new NullLogger<PersistedGrantStore>());
            serializer = new PersistentGrantSerializer();
        }

        public async Task Write(IList<Token> tokens, IList<Consent> consents)
        {
            await WriteTokens(tokens);
            await WriteConsents(consents);
        }

        public async Task WriteConsents(IEnumerable<Consent> consents)
        {
            var handleGenerationService = new DefaultHandleGenerationService();

            var consentStore = new DefaultUserConsentStore(persistedGrantStore, serializer,
                handleGenerationService, new NullLogger<DefaultUserConsentStore>());

            foreach (var consent in consents)
            {
                await consentStore.StoreUserConsentAsync(consent.ToV4Consent());
            }

            await context.SaveChangesAsync();
        }

        public async Task WriteTokens(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                switch (token.TokenType)
                {
                    case TokenType.RefreshToken:
                        await persistedGrantStore.StoreAsync(token.ToV4RefreshTokenAsPersistedGrant(serializer));
                        break;
                    case TokenType.TokenHandle:
                        await persistedGrantStore.StoreAsync(token.ToVersion4ReferenceToken(serializer));
                        break;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}