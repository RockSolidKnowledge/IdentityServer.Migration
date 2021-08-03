using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using IdentityServer3.EntityFramework;
using IdentityServer3.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Rsk.IdentityServer.Migration
{
    public class IdentityServer3ConfigurationDbContext : DbContext
    {
        public IdentityServer3ConfigurationDbContext(DbContextOptions<IdentityServer3ConfigurationDbContext> options) : base(options) { }

        public DbSet<IdentityServer3.EntityFramework.Entities.Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().ToTable("Clients");
            //modelBuilder.Entity<Client>().HasMany<ClientSecret>((Expression<Func<Client, ICollection<ClientSecret>>>)(x => x.ClientSecrets)).WithOne((Expression<Func<ClientSecret, Client>>)(x => x.Client)).OnDelete(DeleteBehavior.Cascade);
            //modelBuilder.Entity<Client>().HasMany<ClientRedirectUri>((Expression<Func<Client, ICollection<ClientRedirectUri>>>)(x => x.RedirectUris)).WithRequired((Expression<Func<ClientRedirectUri, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientPostLogoutRedirectUri>((Expression<Func<Client, ICollection<ClientPostLogoutRedirectUri>>>)(x => x.PostLogoutRedirectUris)).WithRequired((Expression<Func<ClientPostLogoutRedirectUri, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientScope>((Expression<Func<Client, ICollection<ClientScope>>>)(x => x.AllowedScopes)).WithRequired((Expression<Func<ClientScope, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientIdPRestriction>((Expression<Func<Client, ICollection<ClientIdPRestriction>>>)(x => x.IdentityProviderRestrictions)).WithRequired((Expression<Func<ClientIdPRestriction, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientClaim>((Expression<Func<Client, ICollection<ClientClaim>>>)(x => x.Claims)).WithRequired((Expression<Func<ClientClaim, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientCustomGrantType>((Expression<Func<Client, ICollection<ClientCustomGrantType>>>)(x => x.AllowedCustomGrantTypes)).WithRequired((Expression<Func<ClientCustomGrantType, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<Client>().HasMany<ClientCorsOrigin>((Expression<Func<Client, ICollection<ClientCorsOrigin>>>)(x => x.AllowedCorsOrigins)).WithRequired((Expression<Func<ClientCorsOrigin, Client>>)(x => x.Client)).WillCascadeOnDelete();
            //modelBuilder.Entity<ClientSecret>().ToTable("ClientSecrets");
            modelBuilder.Entity<ClientRedirectUri>().ToTable("ClientRedirectUris");
            modelBuilder.Entity<ClientPostLogoutRedirectUri>().ToTable("ClientPostLogoutRedirectUris");
            modelBuilder.Entity<ClientScope>().ToTable("ClientScopes");
            modelBuilder.Entity<ClientIdPRestriction>().ToTable("ClientIdPRestrictions");
            modelBuilder.Entity<ClientClaim>().ToTable("ClientClaims");
            modelBuilder.Entity<ClientCustomGrantType>().ToTable("ClientCustomGrantTypes");
            modelBuilder.Entity<ClientCorsOrigin>().ToTable("ClientCorsOrigins");
        }
    }
}