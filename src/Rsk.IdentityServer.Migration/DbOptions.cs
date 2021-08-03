namespace Rsk.IdentityServer.Migration
{
    public class DbOptions
    {
        public string IdentityServer3ClientsConnectionString { get; set; }
        public string IdentityServer3ScopesConnectionString { get; set; }
        
        public string IdentityServer4ConnectionString { get; set; }
    }
}