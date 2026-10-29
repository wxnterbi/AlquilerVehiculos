using Microsoft.EntityFrameworkCore;
using SistemadeRenta.Domain.Entities;

namespace SistemadeRenta.Persistence.Context
{
    public class SistemadeRentaContext : DbContext
    {
        public SistemadeRentaContext(
            DbContextOptions<SistemadeRentaContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.IdUsuario);
                e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
                e.Property(u => u.Correo).HasMaxLength(150).IsRequired();
                e.Property(u => u.ContrasenaHash).HasMaxLength(500).IsRequired();
                e.Property(u => u.Rol).HasMaxLength(20).IsRequired();
                e.HasIndex(u => u.Correo).IsUnique();
            });
        }
    }
}
