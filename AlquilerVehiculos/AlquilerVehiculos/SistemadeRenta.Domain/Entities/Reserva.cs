using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SistemadeRenta.Domain.Entities
{
    [Table("Reserva")]
    public class Reserva
    {
        [Key]
        public int IdReserva { get; set; }

        public int IdCliente { get; set; }

        public int IdVehiculo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime FechaInicio { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime FechaFin { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal CostoEstimado { get; set; }

        [Required]
        [StringLength(20)]
        public string EstadoReserva { get; set; } = "Pendiente";

        public Cliente Cliente { get; set; } = null!;

        public Vehiculo Vehiculo { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string CodigoSeguimiento { get; set; } = "";
    }
}


