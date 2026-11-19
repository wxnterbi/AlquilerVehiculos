using System.ComponentModel.DataAnnotations;
using SistemadeRenta.Domain.Entities;

namespace SistemadeRenta.Presentation.Models
{
    public class ConsultarReservaViewModel
    {
        [Required(ErrorMessage = "Introduce tu código de seguimiento.")]
        [Display(Name = "Código de seguimiento")]
        public string CodigoSeguimiento { get; set; } = "";

        public Reserva? Reserva { get; set; }
    }
}

