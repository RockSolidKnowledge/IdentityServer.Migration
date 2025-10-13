using System;
using System.IO;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using IdentityServer3.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;

namespace Rsk.IdentityServer.Migration
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            Console.WriteLine("Completed Migration.");
            Console.ReadKey();
        }

        private static async Task MainAsync()
        {
            var provider = ConfigureServices();
            var migrator = provider.GetRequiredService<Migrator>();

            await migrator.Migrate();
        }

        private static ServiceProvider ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();

            var duendeIdentityServerConnectionString = config.GetValue<string>("DuendeIdentityServerConnectionString");

            services.AddDbContext<ConfigurationDbContext>(db => db.UseSqlServer(duendeIdentityServerConnectionString));
            services.AddSingleton(new ConfigurationStoreOptions()
            {
                DefaultSchema = "dbo"
            });
            services.AddSingleton(new OperationalStoreOptions());

            var identityServer3ConnectionString = config.GetValue<string>("IdentityServer3ConnectionString");
            
            services.AddScoped<IClientReader>(_=> new EntityFrameworkClientReader(new ClientConfigurationDbContext(identityServer3ConnectionString)));
            services.AddScoped<IScopeReader>(_=> new EntityFrameworkScopeReader(new ScopeConfigurationDbContext(identityServer3ConnectionString)));

            services.AddScoped<IClientWriter, EntityFrameworkClientWriter>();
            services.AddScoped<IApiResourceWriter, EntityFrameworkApiResourceWriter>();
            services.AddScoped<IIdentityResourceWriter, EntityFrameworkIdentityResourceWriter>();

            services.AddOptions();
            
            services.AddScoped<Migrator>();

            return services.BuildServiceProvider();
        }
    }
}
