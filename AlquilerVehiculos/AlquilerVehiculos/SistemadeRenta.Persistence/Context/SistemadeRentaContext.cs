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
        public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();
        public DbSet<VehiculoImagen> VehiculoImagenes => Set<VehiculoImagen>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(e =>
            {
                e.HasKey(u => u.IdUsuario);
                e.Property(u => u.Nombre).HasMaxLength(100).IsRequired();
                e.Property(u => u.Correo).HasMaxLength(150).IsRequired();
                e.Property(u => u.ContrasenaHash).HasMaxLength(500).IsRequired();
                e.Property(u => u.Rol).HasMaxLength(20).IsRequired();
                e.HasIndex(u => u.Correo).IsUnique();
            });

            modelBuilder.Entity<Vehiculo>(e =>
            {
                e.ToTable("Vehiculo", "dbo");

                e.HasKey(v => v.IdVehiculo);
                e.Property(v => v.IdVehiculo).ValueGeneratedOnAdd();

                e.Property(v => v.Marca).HasMaxLength(80).IsRequired();
                e.Property(v => v.Modelo).HasMaxLength(100).IsRequired();

                e.Property(v => v.Ano)
                    .HasColumnName("Año")
                    .HasConversion<short>()
                    .HasColumnType("smallint");

                e.Property(v => v.Placa).HasMaxLength(20).IsRequired();
                e.HasIndex(v => v.Placa).IsUnique();

                e.Property(v => v.Categoria).HasMaxLength(50).IsRequired();
                e.Property(v => v.Transmision).HasMaxLength(30).IsRequired();
                e.Property(v => v.Estado).HasMaxLength(30).IsRequired();

                e.Property(v => v.PrecioPorDia).HasPrecision(10, 2);
            });

            modelBuilder.Entity<VehiculoImagen>(e =>
            {
                e.ToTable("VehiculoImagen", "dbo");

                e.HasKey(i => i.IdVehiculoImagen);

                e.Property(i => i.IdVehiculoImagen)
                    .ValueGeneratedOnAdd();

                e.Property(i => i.ImagenUrl)
                    .HasMaxLength(500)
                    .IsRequired();

                e.HasIndex(i => new { i.IdVehiculo, i.Orden })
                    .IsUnique();

                e.HasOne<Vehiculo>()
                    .WithMany()
                    .HasForeignKey(i => i.IdVehiculo)
                    .OnDelete(DeleteBehavior.NoAction);
                
                e.Property(i => i.IdPublico)
                      .HasMaxLength(255);

            });

        }
    }
}