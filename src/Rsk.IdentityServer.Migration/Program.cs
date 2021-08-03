using System;
using System.IO;
using System.Threading.Tasks;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rsk.IdentityServer.Migration.Readers;
using Rsk.IdentityServer.Migration.Writers;
using Rsk.IdentityServer.Migration.Writers.Interfaces;

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

            var identityServer4ConnectionString = config.GetValue<string>("IdentityServer4ConnectionString");
            //ServerVersion.AutoDetect(identityServer4ConnectionString)
            services.AddDbContext<ConfigurationDbContext>(db => db.UseSqlServer(identityServer4ConnectionString));
            services.AddSingleton(new ConfigurationStoreOptions());
            services.AddSingleton(new OperationalStoreOptions());

            services.AddScoped<IClientReader, EntityFrameworkClientReader>();
            services.AddScoped<IScopeReader, EntityFrameworkScopeReader>();

            services.AddScoped<IClientWriter, EntityFrameworkClientWriter>();
            services.AddScoped<IApiResourceWriter, EntityFrameworkApiResourceWriter>();
            services.AddScoped<IIdentityResourceWriter, EntityFrameworkIdentityResourceWriter>();

            services.AddOptions();

            var identityServer3ConnectionString = config.GetValue<string>("IdentityServer3ConnectionString");

            services.Configure<DbOptions>(opt =>
                {
                    opt.IdentityServer3ClientsConnectionString = identityServer3ConnectionString;
                    opt.IdentityServer3ScopesConnectionString = identityServer3ConnectionString;
                    opt.IdentityServer4ConnectionString = identityServer4ConnectionString;
                });

            services.AddScoped<Migrator>();

            return services.BuildServiceProvider();
        }
    }
}
