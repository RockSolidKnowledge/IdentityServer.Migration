namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class SecretMappers
    {
        public static Duende.IdentityServer.Models.Secret ToDuende(this IdentityServer3.EntityFramework.Entities.ClientSecret secret)
        {
            return new()
            {
                Type = secret.Type,
                Value = secret.Value,
                Description = secret.Description,
                Expiration = secret.Expiration?.DateTime
            };
        }

        public static Duende.IdentityServer.Models.Secret ToDuende(this IdentityServer3.EntityFramework.Entities.ScopeSecret secret)
        {
            return new()
            {
                Type = secret.Type,
                Value = secret.Value,
                Description = secret.Description,
                Expiration = secret.Expiration?.DateTime
            };
        }
    }
}