using System;
using System.Collections.Generic;
using System.Text;

namespace SistemadeRenta.Domain.Entities
{
    public class Vehiculo
    {
        public int IdVehiculo { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public int Ano { get; set; }
        public string Placa { get; set; }
        public string Categoria { get; set; } // Ejemplo: SUV, Sedan, Compacto
        public string Transmision { get; set; } // Automática / Manual
        public decimal PrecioPorDia { get; set; }
        public string Estado { get; set; } // Disponible, Alquilado, Mantenimiento
    }
}
