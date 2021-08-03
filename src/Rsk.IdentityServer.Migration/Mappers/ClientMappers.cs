using System;
using System.Collections.Generic;
using System.Linq;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityServer3.Core.Models;
using AccessTokenType = IdentityServer3.Core.Models.AccessTokenType;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class ClientMappers
    {
        public static Duende.IdentityServer.Models.Client ToDuende(this IdentityServer3.EntityFramework.Entities.Client client)
        {
            if (client == null) return null;

            var mappedClient = new Duende.IdentityServer.Models.Client();

            mappedClient.AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime;
            mappedClient.AccessTokenLifetime = client.AccessTokenLifetime;
            mappedClient.AccessTokenType = client.AccessTokenType == AccessTokenType.Jwt
                ? Duende.IdentityServer.Models.AccessTokenType.Jwt
                : Duende.IdentityServer.Models.AccessTokenType.Reference;
            mappedClient.AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser;
            mappedClient.AllowOfflineAccess = client.AllowedScopes != null && client.AllowedScopes.Any(x => x.Scope == "offline_access");
            mappedClient.AllowPlainTextPkce = false; // not an option in IdentityServer 3
            mappedClient.AllowRememberConsent = client.AllowRememberConsent;
            mappedClient.AllowedCorsOrigins = client.AllowedCorsOrigins?.Select(x => x.Origin).ToList();
            mappedClient.AllowedGrantTypes = client.Flow.ToGrantTypes();
            mappedClient.AlwaysIncludeUserClaimsInIdToken = false; // not an option per client in IdentityServer 3
            mappedClient.AlwaysSendClientClaims = client.AlwaysSendClientClaims;
            mappedClient.AuthorizationCodeLifetime = client.AuthorizationCodeLifetime;
            mappedClient.Claims = client.Claims?.Select(x => new ClientClaim(x.Type, x.Value)).ToList();
            mappedClient.ClientName = client.ClientName;
            mappedClient.ClientId = client.ClientId;
            mappedClient.ClientClaimsPrefix = client.PrefixClientClaims ? "client_" : null;
            mappedClient.ClientSecrets = client.ClientSecrets?.Select(x => x.ToDuende()).ToList();
            mappedClient.ClientUri = client.ClientUri;
            mappedClient.Enabled = client.Enabled;
            mappedClient.EnableLocalLogin = client.EnableLocalLogin;
            mappedClient.FrontChannelLogoutSessionRequired = client.LogoutSessionRequired;
            mappedClient.FrontChannelLogoutUri = client.LogoutUri;
            mappedClient.IdentityProviderRestrictions =
                client.IdentityProviderRestrictions?.Select(x => x.Provider).ToList();
            mappedClient.IdentityTokenLifetime = client.IdentityTokenLifetime;
            mappedClient.IncludeJwtId = client.IncludeJwtId;
            mappedClient.LogoUri = client.LogoUri;
            mappedClient.PostLogoutRedirectUris = client.PostLogoutRedirectUris?.Select(x => x.Uri).ToList();
            mappedClient.ProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect;
            mappedClient.RedirectUris = client.RedirectUris?.Select(x => x.Uri).ToList();
            mappedClient.RefreshTokenExpiration = client.RefreshTokenExpiration == IdentityServer3.Core.Models.TokenExpiration.Absolute
                ? Duende.IdentityServer.Models.TokenExpiration.Absolute
                : Duende.IdentityServer.Models.TokenExpiration.Sliding;
            mappedClient.RefreshTokenUsage = client.RefreshTokenUsage == IdentityServer3.Core.Models.TokenUsage.OneTimeOnly
                    ? Duende.IdentityServer.Models.TokenUsage.OneTimeOnly
                    : Duende.IdentityServer.Models.TokenUsage.ReUse;
            mappedClient.RequireClientSecret = true; // not an option in IdentityServer 3
            mappedClient.RequireConsent = client.RequireConsent;
            mappedClient.RequirePkce = client.Flow == Flows.AuthorizationCodeWithProofKey ||
                                       client.Flow == Flows.HybridWithProofKey;
            mappedClient.SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime;
            mappedClient.UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenOnRefresh;

            mappedClient.AllowedScopes = client.AllowedScopes?.Select(x => x.Scope).ToList();
            mappedClient.AllowedScopes?.Remove("offline_access");
            if (client.Flow == Flows.Custom && client.AllowedCustomGrantTypes != null && client.AllowedCustomGrantTypes.Any())
                mappedClient.AllowedGrantTypes = client.AllowedCustomGrantTypes?.Select(x => x.GrantType).ToList();

            return mappedClient;
        }

        private static ICollection<string> ToGrantTypes(this Flows flow)
        {
            switch (flow)
            {
                case Flows.Implicit:
                    return GrantTypes.Implicit;
                case Flows.AuthorizationCode:
                case Flows.AuthorizationCodeWithProofKey:
                    return GrantTypes.Code;
                case Flows.Hybrid:
                case Flows.HybridWithProofKey:
                    return GrantTypes.Hybrid;
                case Flows.ClientCredentials:
                    return GrantTypes.ClientCredentials;
                case Flows.ResourceOwner:
                    return GrantTypes.ResourceOwnerPassword;
                case Flows.Custom:
                    return new List<string> { "custom" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(flow), flow, "Invalid value for Flow type");
            }
        }
    }
}
