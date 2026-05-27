using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Data
{
    public class AppDbContext : IdentityUserContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<WorkspaceUsage> WorkspaceUsages { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<PresupuestoItem> PresupuestoItems { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Workspace>()
                .HasOne(w => w.User)
                .WithOne()
                .HasForeignKey<Workspace>(w => w.UserId);

            builder.Entity<Workspace>()
                .HasMany<Company>()
                .WithOne(c => c.Workspace)
                .HasForeignKey(c => c.WorkspaceId);

            builder.Entity<Subscription>()
                .HasOne(s => s.Workspace)
                .WithOne()
                .HasForeignKey<Subscription>(s => s.WorkspaceId);

            builder.Entity<Subscription>()
                .HasOne(s => s.Plan)
                .WithMany()
                .HasForeignKey(s => s.PlanId);

            builder.Entity<Presupuesto>()
                .Property(p => p.Total)
                .HasPrecision(18, 2);

            builder.Entity<PresupuestoItem>()
                .Property(i => i.Materials)
                .HasPrecision(18, 2);

            builder.Entity<PresupuestoItem>()
                .Property(i => i.Labor)
                .HasPrecision(18, 2);

            builder.Entity<PresupuestoItem>()
                .Property(i => i.Subtotal)
                .HasPrecision(18, 2);

            builder.Entity<Plan>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
                
            builder.Entity<WorkspaceUsage>()
                .HasOne(wu => wu.Workspace)
                .WithOne()
                .HasForeignKey<WorkspaceUsage>(wu => wu.WorkspaceId);
        }
    }
}
