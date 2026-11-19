using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SistemadeRenta.Domain.Entities;
using SistemadeRenta.Persistence.Context;
using SistemadeRenta.Presentation.Models;

namespace SistemadeRenta.Presentation.Controllers
{
    public class ReservasController : Controller
    {
        private readonly SistemadeRentaContext _context;

        public ReservasController(SistemadeRentaContext context)
        {
            _context = context;
        }

        // Catalogo publico

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculos
                .AsNoTracking()
                .Where(v => v.Estado == "Disponible")
                .ToListAsync();

            var ids = vehiculos.Select(v => v.IdVehiculo).ToList();

            var imagenes = await _context.VehiculoImagenes
                .AsNoTracking()
                .Where(i => ids.Contains(i.IdVehiculo))
                .OrderBy(i => i.Orden)
                .ToListAsync();

            var model = vehiculos.Select(v =>
                new VehiculoConFotosViewModel
                {
                    Vehiculo = v,
                    Fotos = imagenes
                        .Where(i => i.IdVehiculo == v.IdVehiculo)
                        .Select(i => i.ImagenUrl)
                        .ToList()
                }).ToList();

            return View(model);
        }

        //Detalles del Vehiculo

        public async Task<IActionResult> Detalles(int id)
        {
            var vehiculo = await _context.Vehiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVehiculo == id);

            if (vehiculo == null)
                return NotFound();

            var fotos = await _context.VehiculoImagenes
                .AsNoTracking()
                .Where(i => i.IdVehiculo == id)
                .OrderBy(i => i.Orden)
                .Select(i => i.ImagenUrl)
                .ToListAsync();

            var model = new VehiculoConFotosViewModel
            {
                Vehiculo = vehiculo,
                Fotos = fotos
            };

            return View(model);
        }

        //Formulario de Reserva

        [HttpGet]
        public async Task<IActionResult> Crear(int id)
        {
            var vehiculo = await _context.Vehiculos
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.IdVehiculo == id);

            if (vehiculo == null)
                return NotFound();

            if (vehiculo.Estado != "Disponible")
                return RedirectToAction(nameof(Index));

            var model = new CrearReservaViewModel
            {
                IdVehiculo = vehiculo.IdVehiculo,
                NombreVehiculo = vehiculo.Marca + " " + vehiculo.Modelo,
                PrecioPorDia = vehiculo.PrecioPorDia,
                FechaInicio = DateTime.Now.AddDays(1),
                FechaFin = DateTime.Now.AddDays(2)
            };

            return View(model);
        }

        // Guardar solicitud

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearReservaViewModel model)
        {
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.IdVehiculo == model.IdVehiculo);

            if (vehiculo == null)
                return NotFound();

            model.NombreVehiculo = vehiculo.Marca + " " + vehiculo.Modelo;
            model.PrecioPorDia = vehiculo.PrecioPorDia;

            if (vehiculo.Estado != "Disponible")
            {
                ModelState.AddModelError("", "Este vehículo no está disponible.");
            }

            if (model.FechaInicio <= DateTime.Now)
            {
                ModelState.AddModelError(
                    nameof(model.FechaInicio),
                    "La fecha de recogida debe ser futura.");
            }

            if (model.FechaFin <= model.FechaInicio)
            {
                ModelState.AddModelError(
                    nameof(model.FechaFin),
                    "La devolución debe ser posterior a la recogida.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Comprobar reservas aprobadas
            var reservasExistentes = await _context.Reservas
                .Where(r => r.IdVehiculo == model.IdVehiculo
                    && r.EstadoReserva == "Aprobada")
                .ToListAsync();

            bool conflicto = reservasExistentes.Any(r =>
                model.FechaInicio < r.FechaFin.AddHours(24) &&
                model.FechaFin.AddHours(24) > r.FechaInicio);

            if (conflicto)
            {
                ModelState.AddModelError("",
                    "El vehículo no está disponible en esas fechas.");

                return View(model);
            }

            // Calcular costo
            decimal horas = (decimal)(
                model.FechaFin - model.FechaInicio).TotalHours;

            int dias = (int)Math.Ceiling(horas / 24);

            decimal costo = dias * vehiculo.PrecioPorDia;

            // Buscar cliente por documento
            string documento = model.DocumentoIdentidad.Trim();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c =>
                    c.DocumentoIdentidad == documento);

            // Registrar cliente solamente si no existe
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    IdUsuario = null,
                    Nombre = model.Nombre.Trim(),
                    Apellido = model.Apellido.Trim(),
                    DocumentoIdentidad = documento,
                    EstadoVerificacion = "Pendiente"
                };

                _context.Clientes.Add(cliente);
            }

            // Generar código de seguimiento único
            string codigo;

            do
            {
                codigo = "RES-" +
                    Guid.NewGuid().ToString("N")[..12].ToUpperInvariant();

            } while (await _context.Reservas.AnyAsync(
                r => r.CodigoSeguimiento == codigo));

            // Crear reserva
            var reserva = new Reserva
            {
                Cliente = cliente,
                IdVehiculo = vehiculo.IdVehiculo,
                FechaInicio = model.FechaInicio,
                FechaFin = model.FechaFin,
                CostoEstimado = costo,
                EstadoReserva = "Pendiente",
                CodigoSeguimiento = codigo
            };

            _context.Reservas.Add(reserva);

            await _context.SaveChangesAsync();

            // Enviar datos a la pantalla de confirmación
            TempData["CodigoSeguimiento"] = codigo;
            TempData["CostoReserva"] = costo.ToString("C");

            return RedirectToAction(nameof(Confirmacion));
        }

        // Confirmacion para el cliente

        [HttpGet]
        public IActionResult Confirmacion()
        {
            return View();
        }

        // Panel de administrador

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> Solicitudes()
        {
            var solicitudes = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Cliente)
                .Include(r => r.Vehiculo)
                .Where(r => r.EstadoReserva == "Pendiente")
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();

            return View(solicitudes);
        }

        // Aprovar Solicitud

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound();

            if (reserva.EstadoReserva != "Pendiente")
            {
                TempData["Mensaje"] =
                    "Esta solicitud ya fue procesada.";

                return RedirectToAction(nameof(Solicitudes));
            }

            var aprobadas = await _context.Reservas
                .Where(r =>
                    r.IdVehiculo == reserva.IdVehiculo &&
                    r.IdReserva != id &&
                    r.EstadoReserva == "Aprobada")
                .ToListAsync();

            bool conflicto = aprobadas.Any(r =>
                reserva.FechaInicio < r.FechaFin.AddHours(24) &&
                reserva.FechaFin.AddHours(24) > r.FechaInicio);

            if (conflicto)
            {
                TempData["Mensaje"] =
                    "No se puede aprobar: existe otra reserva aprobada en esas fechas.";

                return RedirectToAction(nameof(Solicitudes));
            }

            reserva.EstadoReserva = "Aprobada";

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La solicitud fue aprobada correctamente.";

            return RedirectToAction(nameof(Solicitudes));
        }

        // Rechazar Solicitud


        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id)
        {
            var reserva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.IdReserva == id);

            if (reserva == null)
                return NotFound();

            if (reserva.EstadoReserva != "Pendiente")
            {
                TempData["Mensaje"] =
                    "Esta solicitud ya fue procesada.";

                return RedirectToAction(nameof(Solicitudes));
            }

            reserva.EstadoReserva = "Rechazada";

            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "La solicitud fue rechazada correctamente.";

            return RedirectToAction(nameof(Solicitudes));
        }


        [HttpGet]
        public IActionResult Consultar()
        {
            return View(new ConsultarReservaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Consultar(ConsultarReservaViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var codigo = model.CodigoSeguimiento.Trim();

            model.Reserva = await _context.Reservas
                .AsNoTracking()
                .Include(r => r.Cliente)
                .Include(r => r.Vehiculo)
                .FirstOrDefaultAsync(r => r.CodigoSeguimiento == codigo);

            if (model.Reserva == null)
            {
                ModelState.AddModelError(
                    nameof(model.CodigoSeguimiento),
                    "No encontramos ninguna solicitud con ese código.");
            }

            return View(model);
        }

    }
}
