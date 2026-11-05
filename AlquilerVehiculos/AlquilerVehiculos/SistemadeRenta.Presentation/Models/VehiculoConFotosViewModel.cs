using SistemadeRenta.Domain.Entities;

namespace SistemadeRenta.Presentation.Models
{
    public class VehiculoConFotosViewModel
    {
        public Vehiculo Vehiculo { get; set; } = new();

        public List<string> Fotos { get; set; } = new();
    }
}