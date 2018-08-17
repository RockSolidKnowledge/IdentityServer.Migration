using System;
using System.IO;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
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
            MainAsync(args).GetAwaiter().GetResult();
            Console.ReadKey();
        }

        public static async Task MainAsync(string[] args)
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

            services.AddDbContext<ConfigurationDbContext>();

            services.AddScoped<IClientReader, EntityFrameworkClientReader>();
            services.AddScoped<IScopeReader, EntityFrameworkScopeReader>();

            services.AddScoped<IClientWriter, EntityFrameworkClientWriter>();
            services.AddScoped<IApiResourceWriter, EntityFrameworkApiResourceWriter>();
            services.AddScoped<IIdentityResourceWriter, EntityFrameworkIdentityResourceWriter>();

            services.AddOptions();
            services.Configure<DbOptions>(config);

            services.AddScoped<Migrator>();

            return services.BuildServiceProvider();
        }
    }
}
