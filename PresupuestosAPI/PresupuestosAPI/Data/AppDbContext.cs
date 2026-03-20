using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<PresupuestoSeccion> PresupuestoSecciones { get; set; }
    }
}
