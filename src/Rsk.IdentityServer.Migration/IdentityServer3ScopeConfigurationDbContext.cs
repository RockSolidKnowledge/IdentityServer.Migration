using IdentityServer3.EntityFramework;
using IdentityServer3.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Rsk.IdentityServer.Migration
{
    public class IdentityServer3ScopeConfigurationDbContext : DbContext
    {
        public IdentityServer3ScopeConfigurationDbContext(DbContextOptions<IdentityServer3ScopeConfigurationDbContext> options) : base(options) { }

        public DbSet<Scope> Scopes { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Scope>().HasKey(x => x.Id);
            modelBuilder.Entity<Scope>().ToTable("Scopes");

            //modelBuilder.Entity<ScopeClaim>().HasOne(x => x.Scope).WithMany(x => x.ScopeClaims).HasForeignKey("Scope_Id");
            
            modelBuilder.Entity<Scope>().HasMany<ScopeClaim>().WithOne(x => x.Scope).HasForeignKey("ScopeId").IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Scope>().HasMany<ScopeSecret>().WithOne(x => x.Scope).HasForeignKey("ScopeId").IsRequired().OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ScopeClaim>().ToTable("ScopeClaims");
            modelBuilder.Entity<ScopeSecret>().ToTable("ScopeSecrets");



            //modelBuilder.Entity<ScopeClaim>().Property(x => x.Scope).has("Scope_Id");

            //modelBuilder.Entity<Scope>().HasMany<ScopeClaim>((Expression<Func<Scope, ICollection<ScopeClaim>>>) (x => x.ScopeClaims)).WithRequired((Expression<Func<ScopeClaim, Scope>>) (x => x.Scope)).WillCascadeOnDelete();
            //modelBuilder.Entity<Scope>().HasMany<ScopeSecret>((Expression<Func<Scope, ICollection<ScopeSecret>>>) (x => x.ScopeSecrets)).WithRequired((Expression<Func<ScopeSecret, Scope>>) (x => x.Scope)).WillCascadeOnDelete();

            //ScopeConfigurationDbContext
        }
    }
}