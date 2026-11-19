using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemadeRenta.Persistence.Context;

public class PagosController : Controller
{
    private readonly SistemadeRentaContext _context;

    public PagosController(SistemadeRentaContext context)
    {
        _context = context;
    }

    // Lista reservas aceptadas pendientes de cobro + historial de pagos
    public async Task<IActionResult> Index()
    {
        var reservasConContrato = await _context.AlquilerContratos
            .Select(c => c.IdReserva)
            .ToListAsync();

        var reservasAceptadas = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .Where(r => (r.EstadoReserva == "Aprobada")
                        && !reservasConContrato.Contains(r.IdReserva))
            .OrderBy(r => r.FechaInicio)
            .ToListAsync();

        var pagosRealizados = await _context.Pagos
            .Include(p => p.AlquilerContrato)
                .ThenInclude(c => c.Reserva)
                    .ThenInclude(r => r.Cliente)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        ViewBag.PagosRealizados = pagosRealizados;
        return View(reservasAceptadas);
    }

    // GET: /Pagos/Registrar?idReserva=5
    public async Task<IActionResult> Registrar(int idReserva)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

        if (reserva == null)
            return NotFound();

        if (reserva.EstadoReserva != "Aprobada")
        {
            TempData["Error"] = "Solo se pueden registrar pagos de reservas aprobada.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Reserva = reserva;

        var modelo = new Pago
        {
            Monto = reserva.CostoEstimado,
            MetodoPago = "Efectivo",
            TipoPago = "Alquiler",
            Estado = "Completado",
            FechaPago = DateTime.Now
        };

        ViewData["IdReserva"] = reserva.IdReserva;
        return View(modelo);
    }

    // POST: /Pagos/Registrar
    // POST: /Pagos/Registrar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int idReserva, Pago pago)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Cliente)
            .Include(r => r.Vehiculo)
            .FirstOrDefaultAsync(r => r.IdReserva == idReserva);

        if (reserva == null)
            return NotFound();

        pago.Estado = string.IsNullOrWhiteSpace(pago.Estado) ? "Completado" : pago.Estado;
        pago.FechaPago = pago.FechaPago == default ? DateTime.Now : pago.FechaPago;

        ModelState.Remove(nameof(Pago.AlquilerContrato));
        ModelState.Remove(nameof(Pago.Estado));
        ModelState.Remove(nameof(Pago.FechaPago));

        if (reserva.EstadoReserva != "Aprobada")
        {
            ModelState.AddModelError("", "La reserva no está en estado Aprobada.");
        }

        if (pago.Monto <= 0)
        {
            ModelState.AddModelError(nameof(pago.Monto), "El monto debe ser mayor que cero.");
        }

        int idEmpleado = await _context.Database
            .SqlQueryRaw<int>("SELECT TOP 1 IdEmpleado AS [Value] FROM dbo.Empleado")
            .FirstOrDefaultAsync();

        if (idEmpleado == 0)
        {
            ModelState.AddModelError("", "No hay ningún empleado registrado en dbo.Empleado para asignar al contrato.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Reserva = reserva;
            ViewData["IdReserva"] = reserva.IdReserva;
            return View(pago);
        }

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var contrato = new AlquilerContrato
            {
                IdCliente = reserva.IdCliente,
                IdEmpleado = idEmpleado,
                IdVehiculo = reserva.IdVehiculo,
                IdReserva = reserva.IdReserva,
                FechaSalida = reserva.FechaInicio,
                FechaDevolucion = reserva.FechaFin,
                MontoTotal = pago.Monto,
                Estado = "Activo"
            };

            _context.AlquilerContratos.Add(contrato);
            await _context.SaveChangesAsync();

            pago.IdContrato = contrato.IdContrato;
            _context.Pagos.Add(pago);

            reserva.EstadoReserva = "Pagada";
            _context.Reservas.Update(reserva);

            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["Success"] = $"Pago registrado con éxito para el código {reserva.CodigoSeguimiento}.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            ModelState.AddModelError("", $"Error al registrar el pago: {ex.InnerException?.Message ?? ex.Message}");
            ViewBag.Reserva = reserva;
            ViewData["IdReserva"] = reserva.IdReserva;
            return View(pago);
        }
    }
}