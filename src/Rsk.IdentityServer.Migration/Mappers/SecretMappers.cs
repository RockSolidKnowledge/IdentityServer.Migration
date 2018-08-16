namespace Rsk.IdentityServer.Migration.Mappers
{
    public static class SecretMappers
    {
        public static IdentityServer4.Models.Secret ToVersion4(this IdentityServer3.Core.Models.Secret secret)
        {
            return new IdentityServer4.Models.Secret
            {
                Type = secret.Type,
                Value = secret.Value,
                Description = secret.Description,
                Expiration = secret.Expiration?.DateTime
            };
        }
    }
}