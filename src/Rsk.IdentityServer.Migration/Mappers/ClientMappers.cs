using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityServer3.Core.Models;
using AccessTokenType = IdentityServer3.Core.Models.AccessTokenType;
using Client = IdentityServer3.Core.Models.Client;

namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class ClientMappers
    {
        public static Duende.IdentityServer.Models.Client ToVersion4(this Client client)
        {
            if (client == null) return null;

            var mappedClient = new Duende.IdentityServer.Models.Client
            {
                AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                AccessTokenLifetime = client.AccessTokenLifetime,
                AccessTokenType = client.AccessTokenType == AccessTokenType.Jwt
                    ? Duende.IdentityServer.Models.AccessTokenType.Jwt
                    : Duende.IdentityServer.Models.AccessTokenType.Reference,
                AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                AllowOfflineAccess = client.AllowedScopes.Contains("offline_access"),
                AllowPlainTextPkce = false, // not an option in IdentityServer 3
                AllowRememberConsent = client.AllowRememberConsent,
                AllowedCorsOrigins = client.AllowedCorsOrigins,
                AllowedGrantTypes = client.Flow.ToGrantTypes(),
                AllowedScopes = client.AllowedScopes,
                AlwaysIncludeUserClaimsInIdToken = false, // not an option per client in IdentityServer 3
                AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                Claims = ToClientClaims(client.Claims),
                ClientName = client.ClientName,
                ClientId = client.ClientId,
                ClientClaimsPrefix = client.PrefixClientClaims ? "client_" : null,
                ClientSecrets = client.ClientSecrets.Select(x => x.ToDuende()).ToList(),
                ClientUri = client.ClientUri,
                Enabled = client.Enabled,
                EnableLocalLogin = client.EnableLocalLogin,
                FrontChannelLogoutSessionRequired = client.LogoutSessionRequired,
                FrontChannelLogoutUri = client.LogoutUri,
                IdentityProviderRestrictions = client.IdentityProviderRestrictions,
                IdentityTokenLifetime = client.IdentityTokenLifetime,
                IncludeJwtId = client.IncludeJwtId,
                LogoUri = client.LogoUri,
                PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                ProtocolType = IdentityServerConstants.ProtocolTypes.OpenIdConnect,
                RedirectUris = client.RedirectUris,
                RefreshTokenExpiration = client.RefreshTokenExpiration == IdentityServer3.Core.Models.TokenExpiration.Absolute
                    ? Duende.IdentityServer.Models.TokenExpiration.Absolute
                    : Duende.IdentityServer.Models.TokenExpiration.Sliding,
                RefreshTokenUsage = client.RefreshTokenUsage == IdentityServer3.Core.Models.TokenUsage.OneTimeOnly
                    ? Duende.IdentityServer.Models.TokenUsage.OneTimeOnly
                    : Duende.IdentityServer.Models.TokenUsage.ReUse,
                RequireClientSecret = true, // not an option in IdentityServer 3
                RequireConsent = client.RequireConsent,
                RequirePkce = client.Flow == Flows.AuthorizationCodeWithProofKey || client.Flow == Flows.HybridWithProofKey,
                SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh
            };

            mappedClient.AllowedScopes.Remove("offline_access");
            if (client.Flow == Flows.Custom && client.AllowedCustomGrantTypes != null && client.AllowedCustomGrantTypes.Any())
                mappedClient.AllowedGrantTypes = client.AllowedCustomGrantTypes;
            
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

        private static ICollection<ClientClaim> ToClientClaims(this List<Claim> claims)
        {
            return claims.Select(x => new ClientClaim(x.Type, x.Value, x.ValueType)).ToList();
        }
    }
}
