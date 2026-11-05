using System;
using System.Collections.Generic;
using System.Text;

namespace SistemadeRenta.Domain.Entities
{
    public class VehiculoImagen
    {
        public int IdVehiculoImagen { get; set; }

        public int IdVehiculo { get; set; }

        public string ImagenUrl { get; set; } = string.Empty;

        public int Orden { get; set; }

        public string? IdPublico { get; set; }

    }

}

