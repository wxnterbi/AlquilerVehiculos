using System;
using System.ComponentModel.DataAnnotations;

namespace SistemadeRenta.Presentation.Models
{
    public class CrearReservaViewModel
    {
        public int IdVehiculo { get; set; }

        public string NombreVehiculo { get; set; } = "";

        public decimal PrecioPorDia { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100)]
        [Display(Name = "Apellido")]
        public string Apellido { get; set; } = "";

        [Required(ErrorMessage = "El documento de identidad es obligatorio.")]
        [StringLength(50)]
        [Display(Name = "Documento de identidad")]
        public string DocumentoIdentidad { get; set; } = "";

        public bool ClienteRegistrado { get; set; } = false;

        [Required(ErrorMessage = "Selecciona la fecha y hora de recogida.")]
        [Display(Name = "Fecha y hora de recogida")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "Selecciona la fecha y hora de devolución.")]
        [Display(Name = "Fecha y hora de devolución")]
        public DateTime FechaFin { get; set; }

        public decimal CostoEstimado { get; set; }
    }
}


