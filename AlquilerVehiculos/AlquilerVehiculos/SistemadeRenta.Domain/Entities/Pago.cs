using SistemadeRenta.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Pago")]
public class Pago
{
    [Key]
    public int IdPago { get; set; }

    public int IdContrato { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Monto { get; set; }

    [Required]
    [StringLength(30)]
    public string Estado { get; set; } = "Completado";

    [Required]
    [StringLength(30)]
    public string MetodoPago { get; set; } = "Efectivo";

    [Required]
    [StringLength(30)]
    public string TipoPago { get; set; } = "Alquiler";

    [Column(TypeName = "datetime2")]
    public DateTime FechaPago { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdContrato))]
    public virtual AlquilerContrato AlquilerContrato { get; set; } = null!;
}