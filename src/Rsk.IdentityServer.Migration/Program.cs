using System;
using System.IO;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Options;
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

            services.AddDbContext<ConfigurationDbContext>(db => db.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=migration.test.output;Integrated Security=SSPI;"));
            services.AddSingleton(new ConfigurationStoreOptions());

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
