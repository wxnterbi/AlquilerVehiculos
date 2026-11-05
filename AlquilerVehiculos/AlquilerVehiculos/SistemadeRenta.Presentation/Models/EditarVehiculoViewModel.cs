using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SistemadeRenta.Presentation.Models
{
    public class EditarVehiculoViewModel
    {
        public int IdVehiculo { get; set; }

        [Required(ErrorMessage = "Escribe la marca.")]
        [StringLength(80)]
        public string Marca { get; set; } = string.Empty;

        [Required(ErrorMessage = "Escribe el modelo.")]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "Año")]
        [Range(2010, 2100,
            ErrorMessage = "Escribe un año entre 2010 y 2100.")]
        public int Ano { get; set; }

        [Required(ErrorMessage = "Escribe la placa.")]
        [StringLength(20)]
        public string Placa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona la categoría.")]
        [StringLength(50)]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona la transmisión.")]
        [StringLength(30)]
        [Display(Name = "Transmisión")]
        public string Transmision { get; set; } = string.Empty;

        [Display(Name = "Precio por día")]
        [Range(typeof(decimal), "1.00", "99999999.99",
            ErrorMessage = "El precio debe estar entre 1.00 y 99,999,999.99.",
            ParseLimitsInInvariantCulture = true)]
        public decimal PrecioPorDia { get; set; }

        [Required(ErrorMessage = "Selecciona el estado.")]
        [RegularExpression(
            "^(Disponible|No disponible)$",
            ErrorMessage = "Selecciona un estado válido.")]
        public string Estado { get; set; } = "Disponible";

        [Display(Name = "Agregar fotos")]
        [MaxLength(10,
            ErrorMessage = "Puedes agregar un máximo de 10 fotos nuevas.")]
        public List<IFormFile> FotosNuevas { get; set; } = new();

        // Fotos que ya están guardadas.
        public List<FotoVehiculoEdicionViewModel> FotosActuales
        { get; set; } = new();

        // Fotos existentes que el administrador desea quitar.
        public List<int> FotosEliminar { get; set; } = new();

        // Foto existente que se desea colocar como principal.
        public int? IdFotoPrincipal { get; set; }
    }

    public class FotoVehiculoEdicionViewModel
    {
        public int IdVehiculoImagen { get; set; }

        public string ImagenUrl { get; set; } = string.Empty;

        public int Orden { get; set; }
    }
}