using SistemadeRenta.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AlquilerContrato")]
public class AlquilerContrato
{
    [Key]
    public int IdContrato { get; set; }

    public int IdCliente { get; set; }
    public int IdEmpleado { get; set; }
    public int IdVehiculo { get; set; }
    public int IdReserva { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime FechaSalida { get; set; }

    [Column(TypeName = "datetime2")]
    public DateTime FechaDevolucion { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal MontoTotal { get; set; }

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Activo";

    [ForeignKey(nameof(IdReserva))]
    public virtual Reserva Reserva { get; set; } = null!;
}