using System;
using System.Collections.Generic;
using System.Text;

namespace SistemadeRenta.Domain.Entities
{
    public class Vehiculo
    {
        public int IdVehiculo { get; set; }

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public int Ano { get; set; }

        public string Placa { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public string Transmision { get; set; } = string.Empty;

        public decimal PrecioPorDia { get; set; }

        public string Estado { get; set; } = "Disponible";
    }
}
