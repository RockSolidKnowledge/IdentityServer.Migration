namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class SecretMappers
    {
        public static Duende.IdentityServer.Models.Secret ToDuende(this IdentityServer3.Core.Models.Secret secret)
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