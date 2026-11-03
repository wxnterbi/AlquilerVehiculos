using System;
using System.Collections.Generic;
using System.Text;

namespace SistemadeRenta.Domain.Entities
{
    public class Pago
    {
        public int IdPago { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string? MetodoPago { get; set; } 
        public string? EstadoPago { get; set; } 
        public int IdAlquiler { get; set; }
        public Alquiler? Alquiler { get; set; }
    }
}
