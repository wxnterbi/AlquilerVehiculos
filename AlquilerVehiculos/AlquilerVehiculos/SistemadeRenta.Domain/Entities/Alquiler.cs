using System;
using System.Collections.Generic;
using System.Text;

namespace SistemadeRenta.Domain.Entities
{
    public class Alquiler
    {
        public int IdAlquiler { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaDevolucionEstimada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }
        public decimal CostoTotal { get; set; }
        public string? EstadoAlquiler { get; set; }
        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }
        public int IdVehiculo { get; set; }
        public Vehiculo Vehiculo { get; set; }
    }
}