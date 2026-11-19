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
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Reserva> Reservas { get; set; }


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


            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(e =>
            {
                e.ToTable("Cliente", "dbo");

                e.HasKey(c => c.IdCliente);

                e.Property(c => c.IdUsuario)
                    .IsRequired(false);

                e.Property(c => c.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(c => c.Apellido)
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(c => c.DocumentoIdentidad)
                    .HasMaxLength(50)
                    .IsRequired();

                e.Property(c => c.EstadoVerificacion)
                    .HasMaxLength(30)
                    .IsRequired();

                e.HasOne(c => c.Usuario)
                    .WithMany()
                    .HasForeignKey(c => c.IdUsuario)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // Configuración de Reserva
            modelBuilder.Entity<Reserva>(e =>
            {
                e.ToTable("Reserva", "dbo");

                e.HasKey(r => r.IdReserva);

                e.HasOne(r => r.Cliente)
                    .WithMany()
                    .HasForeignKey(r => r.IdCliente)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(r => r.Vehiculo)
                    .WithMany()
                    .HasForeignKey(r => r.IdVehiculo)
                    .OnDelete(DeleteBehavior.NoAction);

                e.Property(r => r.CostoEstimado)
                    .HasPrecision(10, 2);

                e.Property(r => r.EstadoReserva)
                   .HasMaxLength(20)
                   .IsRequired();
            });

        }
    }
}