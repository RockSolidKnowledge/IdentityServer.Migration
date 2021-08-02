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

            services.AddDbContext<ConfigurationDbContext>(db => db.UseSqlServer(config.GetValue<string>("IdentityServer4ConnectionString")));
            services.AddDbContext<PersistedGrantDbContext>(db => db.UseSqlServer(config.GetValue<string>("IdentityServer4ConnectionString")));
            services.AddSingleton(new ConfigurationStoreOptions());
            services.AddSingleton(new OperationalStoreOptions());

            services.AddScoped<IClientReader, EntityFrameworkClientReader>();
            services.AddScoped<IScopeReader, EntityFrameworkScopeReader>();
            services.AddScoped<ITokenReader, EntityFrameworkTokenReader>();

            services.AddScoped<IClientWriter, EntityFrameworkClientWriter>();
            services.AddScoped<IApiResourceWriter, EntityFrameworkApiResourceWriter>();
            services.AddScoped<IIdentityResourceWriter, EntityFrameworkIdentityResourceWriter>();
            services.AddScoped<IPersistedGrantsWriter, EntityFrameworkPersistedGrantsWriter>();

            services.AddOptions();
            services.Configure<DbOptions>(opt =>
                {
                    opt.IdentityServer3ClientsConnectionString = config.GetValue<string>("IdentityServer3ConnectionString");
                    opt.IdentityServer3ScopesConnectionString = config.GetValue<string>("IdentityServer3ConnectionString");
                    opt.IdentityServer3OperationalConnectionString = config.GetValue<string>("IdentityServer3ConnectionString");
                    opt.IdentityServer4ConnectionString = config.GetValue<string>("IdentityServer4ConnectionString");
                });

            services.AddScoped<Migrator>();

            return services.BuildServiceProvider();
        }
    }
}
