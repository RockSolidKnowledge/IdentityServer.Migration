using System;
using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores.Serialization;
using Consent = IdentityServer3.EntityFramework.Entities.Consent;
using RefreshToken = IdentityServer3.Core.Models.RefreshToken;
using Token = IdentityServer3.EntityFramework.Entities.Token;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class ConsentMapper
    {
        public static Duende.IdentityServer.Models.Consent ToV4Consent(this Consent consent)
        {
            return new()
            {
                ClientId = consent.ClientId,
                CreationTime = DateTime.UtcNow, // not stored in IdentityServer3 Consent
                Expiration = null,
                Scopes = consent.Scopes.Split(','),
                SubjectId = consent.Subject
            };
        }
        
        public static PersistedGrant ToV4RefreshTokenAsPersistedGrant(this Token token, PersistentGrantSerializer serializer)
        {
            var ids4RefreshToken = serializer.Deserialize<RefreshToken>(token.JsonCode)
                .ToVersion4RefreshToken();
            
            return new PersistedGrant
            {
                Key = token.Key, 
                ClientId = ids4RefreshToken.ClientId,
                CreationTime = ids4RefreshToken.CreationTime,
                Expiration = ids4RefreshToken.CreationTime.AddSeconds(ids4RefreshToken.Lifetime),
                SubjectId = ids4RefreshToken.SubjectId,
                Type = IdentityServerConstants.PersistedGrantTypes.RefreshToken,
                Data = serializer.Serialize(ids4RefreshToken)
            };
        }

        public static Duende.IdentityServer.Models.RefreshToken ToVersion4RefreshToken(this RefreshToken refreshToken)
        {
            return new()
            {
                CreationTime = refreshToken.CreationTime.UtcDateTime,
                AccessToken = new Duende.IdentityServer.Models.Token
                {
                    CreationTime = refreshToken.AccessToken.CreationTime.UtcDateTime,
                    AccessTokenType = AccessTokenType.Jwt,
                    ClientId = refreshToken.AccessToken.ClientId,
                    Audiences = new List<string> { refreshToken.AccessToken.Audience },
                    Type = refreshToken.AccessToken.Type,
                    Claims = refreshToken.AccessToken.Claims,
                    Issuer = refreshToken.AccessToken.Issuer,
                    Lifetime = refreshToken.AccessToken.Lifetime,
                    Version = refreshToken.AccessToken.Version
                },
                Lifetime = refreshToken.LifeTime,
                Version = refreshToken.Version
            };
        }

        public static PersistedGrant ToVersion4ReferenceToken(this Token token, PersistentGrantSerializer serializer)
        {
            var refToken = serializer.Deserialize<IdentityServer3.Core.Models.Token>(token.JsonCode);
            
            var ids4ReferenceToken =  new Duende.IdentityServer.Models.Token
            {
                ClientId = refToken.ClientId,
                CreationTime = refToken.CreationTime.UtcDateTime,
                Audiences = new List<string>{refToken.Audience},
                Lifetime = refToken.Lifetime,
                Type = refToken.Type,
                Claims = refToken.Claims,
                Issuer = refToken.Issuer,
                Version = refToken.Version
            };

            return new PersistedGrant
            {
                Key = token.Key, 
                ClientId = ids4ReferenceToken.ClientId,
                CreationTime = ids4ReferenceToken.CreationTime,
                Expiration = ids4ReferenceToken.CreationTime.AddSeconds(ids4ReferenceToken.Lifetime),
                SubjectId = ids4ReferenceToken.SubjectId,
                Type = IdentityServerConstants.PersistedGrantTypes.ReferenceToken,
                Data = serializer.Serialize(ids4ReferenceToken)
            };
        }
    }
}