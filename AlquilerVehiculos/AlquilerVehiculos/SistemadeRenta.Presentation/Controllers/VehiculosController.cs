using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemadeRenta.Domain.Entities;
using System.Collections.Generic;

namespace SistemadeRenta.Presentation.Controllers
{
    [Authorize]
    public class VehiculosController : Controller
    {
        [HttpGet]
        public IActionResult Catalogo()
        {
            var listaVehiculos = new List<Vehiculo>
            {
                new Vehiculo { IdVehiculo = 1, Marca = "Toyota", Modelo = "Corolla", Ano = 2023, Categoria = "Sedan", Transmision = "Automática", PrecioPorDia = 2500, Estado = "Disponible" },
                new Vehiculo { IdVehiculo = 2, Marca = "Honda", Modelo = "CR-V", Ano = 2022, Categoria = "SUV", Transmision = "Automática", PrecioPorDia = 3800, Estado = "Disponible" },
                new Vehiculo { IdVehiculo = 3, Marca = "Hyundai", Modelo = "Elantra", Ano = 2021, Categoria = "Sedan", Transmision = "Manual", PrecioPorDia = 2200, Estado = "Disponible" }
            };

            return View(listaVehiculos);
        }
    }
}