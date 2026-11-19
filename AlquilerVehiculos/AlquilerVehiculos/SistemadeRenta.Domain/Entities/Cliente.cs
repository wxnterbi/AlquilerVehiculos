using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemadeRenta.Domain.Entities
{
    [Table("Cliente")]
    public class Cliente
    {
        [Key]
        public int IdCliente { get; set; }

        public int? IdUsuario { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string Apellido { get; set; } = "";

        [Required]
        [StringLength(50)]
        public string DocumentoIdentidad { get; set; } = "";

        [Required]
        [StringLength(30)]
        public string EstadoVerificacion { get; set; } = "";

        public Usuario? Usuario { get; set; }
    }
}

