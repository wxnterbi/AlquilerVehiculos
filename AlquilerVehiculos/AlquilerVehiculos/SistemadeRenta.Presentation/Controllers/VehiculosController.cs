using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemadeRenta.Domain.Entities;
using SistemadeRenta.Infrastructure.Services;
using SistemadeRenta.Persistence.Context;
using SistemadeRenta.Presentation.Models; 

namespace SistemadeRenta.Presentation.Controllers
{
    [Authorize]
    public class VehiculosController : Controller
    {
        private readonly SistemadeRentaContext _context;
        private readonly ServicioImagenes _imagenes;
        private readonly ILogger<VehiculosController> _logger;

        public VehiculosController(
            SistemadeRentaContext context,
            ServicioImagenes imagenes,
            ILogger<VehiculosController> logger)
        {
            _context = context;
            _imagenes = imagenes;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Catalogo(string? buscar)
        {
            var consulta = _context.Vehiculos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var termino = buscar.Trim();

                consulta = consulta.Where(v =>
                    v.Marca.Contains(termino) ||
                    v.Modelo.Contains(termino));
            }

            var vehiculos = await consulta
                .OrderBy(v => v.Categoria)
                .ThenBy(v => v.Marca)
                .ThenBy(v => v.Modelo)
                .ToListAsync();

            var ids = vehiculos.Select(v => v.IdVehiculo).ToList();

            var imagenes = await _context.VehiculoImagenes
                .AsNoTracking()
                .Where(i => ids.Contains(i.IdVehiculo))
                .OrderBy(i => i.Orden)
                .Select(i => new
                {
                    i.IdVehiculo,
                    i.ImagenUrl
                })
                .ToListAsync();

            var fotosPorVehiculo = imagenes.ToLookup(i => i.IdVehiculo);

            var model = vehiculos.Select(v => new VehiculoConFotosViewModel
            {
                Vehiculo = v,
                Fotos = fotosPorVehiculo[v.IdVehiculo]
                    .Select(i => i.ImagenUrl)
                    .ToList()
            }).ToList();

            ViewData["Buscar"] = buscar;

            return View(model);
        }

        [HttpGet]
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

            return View(new VehiculoConFotosViewModel
            {
                Vehiculo = vehiculo,
                Fotos = fotos
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id)
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
                .Select(i => new FotoVehiculoEdicionViewModel
                {
                    IdVehiculoImagen = i.IdVehiculoImagen,
                    ImagenUrl = i.ImagenUrl,
                    Orden = i.Orden
                })
                .ToListAsync();

            return View(new EditarVehiculoViewModel
            {
                IdVehiculo = vehiculo.IdVehiculo,
                Marca = vehiculo.Marca,
                Modelo = vehiculo.Modelo,
                Ano = vehiculo.Ano,
                Placa = vehiculo.Placa,
                Categoria = vehiculo.Categoria,
                Transmision = vehiculo.Transmision,
                PrecioPorDia = vehiculo.PrecioPorDia,
                Estado = vehiculo.Estado,
                FotosActuales = fotos,
                IdFotoPrincipal = fotos.FirstOrDefault()?.IdVehiculoImagen
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(55 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 55 * 1024 * 1024)]
        public async Task<IActionResult> Editar(
            int id,
            EditarVehiculoViewModel model)
        {
            if (id != model.IdVehiculo)
                return BadRequest();

            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.IdVehiculo == id);

            if (vehiculo == null)
                return NotFound();

            var fotosNuevas = model.FotosNuevas ?? new List<IFormFile>();

            if (!ModelState.IsValid)
            {
                await CargarFotosEdicionAsync(model);
                return View(model);
            }

            var placa = model.Placa.Trim().ToUpperInvariant();

            if (await _context.Vehiculos.AnyAsync(v =>
                v.IdVehiculo != id && v.Placa == placa))
            {
                ModelState.AddModelError(
                    nameof(model.Placa),
                    "Ya existe otro vehículo con esa placa.");

                await CargarFotosEdicionAsync(model);
                return View(model);
            }

            var fotosActuales = await _context.VehiculoImagenes
                .Where(i => i.IdVehiculo == id)
                .OrderBy(i => i.Orden)
                .ToListAsync();

            var idsEliminar = model.FotosEliminar.Distinct().ToList();

            if (idsEliminar.Any(idFoto =>
                !fotosActuales.Any(f => f.IdVehiculoImagen == idFoto)))
            {
                ModelState.AddModelError(
                    nameof(model.FotosEliminar),
                    "Una de las fotos seleccionadas no pertenece al vehículo.");

                await CargarFotosEdicionAsync(model);
                return View(model);
            }

            var fotosEliminar = fotosActuales
                .Where(f => idsEliminar.Contains(f.IdVehiculoImagen))
                .ToList();

            var totalFinal =
                fotosActuales.Count - fotosEliminar.Count + fotosNuevas.Count;

            if (totalFinal < 5 || totalFinal > 10)
            {
                ModelState.AddModelError(
                    nameof(model.FotosNuevas),
                    "El vehículo debe quedar con entre 5 y 10 fotos.");

                await CargarFotosEdicionAsync(model);
                return View(model);
            }

            // Validamos todas las fotos nuevas antes de subir la primera.
            foreach (var foto in fotosNuevas)
            {
                if (!await EsFotoValidaAsync(foto))
                {
                    ModelState.AddModelError(
                        nameof(model.FotosNuevas),
                        "Las fotos nuevas deben ser JPG o PNG válidos, "
                        + "de hasta 5 MB cada una.");

                    await CargarFotosEdicionAsync(model);
                    return View(model);
                }
            }

            var fotosSubidas = new List<(string Url, string IdPublico)>();

            try
            {
                foreach (var foto in fotosNuevas)
                {
                    using var archivo = foto.OpenReadStream();

                    var extension = Path.GetExtension(foto.FileName)
                        .ToLowerInvariant();

                    var resultado = await _imagenes.SubirAsync(
                        archivo,
                        $"foto{extension}");

                    fotosSubidas.Add(resultado);
                }

                vehiculo.Marca = model.Marca.Trim();
                vehiculo.Modelo = model.Modelo.Trim();
                vehiculo.Ano = model.Ano;
                vehiculo.Placa = placa;
                vehiculo.Categoria = model.Categoria.Trim();
                vehiculo.Transmision = model.Transmision.Trim();
                vehiculo.PrecioPorDia = model.PrecioPorDia;
                vehiculo.Estado = model.Estado;

                // Las fotos nuevas se agregan al final.
                var ultimoOrden = fotosActuales
                    .Select(f => f.Orden)
                    .DefaultIfEmpty(0)
                    .Max();

                foreach (var foto in fotosSubidas)
                {
                    ultimoOrden++;

                    _context.VehiculoImagenes.Add(new VehiculoImagen
                    {
                        IdVehiculo = id,
                        ImagenUrl = foto.Url,
                        IdPublico = foto.IdPublico,
                        Orden = ultimoOrden
                    });
                }

                _context.VehiculoImagenes.RemoveRange(fotosEliminar);

                // Un único guardado para datos, fotos nuevas y eliminaciones.
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falló la edición del vehículo {IdVehiculo}.",
                    id);

                // Retiramos las fotos nuevas si no se completó el guardado.
                foreach (var foto in fotosSubidas)
                {
                    try
                    {
                        await _imagenes.EliminarAsync(foto.IdPublico);
                    }
                    catch (Exception errorLimpieza)
                    {
                        _logger.LogWarning(
                            errorLimpieza,
                            "Quedó pendiente retirar la foto nueva {IdPublico}.",
                            foto.IdPublico);
                    }
                }

                ModelState.AddModelError(
                    "",
                    "No se pudieron guardar los cambios. "
                    + "Inténtalo nuevamente y vuelve a seleccionar las fotos nuevas.");

                await CargarFotosEdicionAsync(model);
                return View(model);
            }

            // Las fotos anteriores se retiran de Cloudinary después de guardar.
            foreach (var foto in fotosEliminar)
            {
                var idPublico = foto.IdPublico;

                if (string.IsNullOrWhiteSpace(idPublico))
                    continue;

                try
                {
                    await _imagenes.EliminarAsync(idPublico);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Quedó pendiente retirar la foto anterior {IdPublico}.",
                        idPublico);
                }
            }

            TempData["Exito"] = "Los cambios se guardaron correctamente.";

            return RedirectToAction(nameof(Detalles), new { id });
        }
        private async Task CargarFotosEdicionAsync(
            EditarVehiculoViewModel model)
        {
            model.FotosActuales = await _context.VehiculoImagenes
                .AsNoTracking()
                .Where(i => i.IdVehiculo == model.IdVehiculo)
                .OrderBy(i => i.Orden)
                .Select(i => new FotoVehiculoEdicionViewModel
                {
                    IdVehiculoImagen = i.IdVehiculoImagen,
                    ImagenUrl = i.ImagenUrl,
                    Orden = i.Orden
                })
                .ToListAsync();

            model.IdFotoPrincipal = model.FotosActuales
                .FirstOrDefault()?.IdVehiculoImagen;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Crear()
        {
            return View(new VehiculoViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(55 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 55 * 1024 * 1024)]
        public async Task<IActionResult> Crear(VehiculoViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var placa = model.Placa.Trim().ToUpperInvariant();

            if (await _context.Vehiculos.AnyAsync(v => v.Placa == placa))
            {
                ModelState.AddModelError(
                    nameof(model.Placa),
                    "Ya existe un vehículo con esa placa.");

                return View(model);
            }

            // Revisamos todas las fotos antes de subir la primera.
            foreach (var foto in model.Fotos)
            {
                if (!await EsFotoValidaAsync(foto))
                {
                    ModelState.AddModelError(
                        nameof(model.Fotos),
                        "Todas las fotos deben ser JPG o PNG válidos, " +
                        "de hasta 5 MB cada una.");

                    return View(model);
                }
            }

            var fotosSubidas = new List<(string Url, string IdPublico)>();

            try
            {
                foreach (var foto in model.Fotos)
                {
                    using var archivo = foto.OpenReadStream();

                    var extension = Path.GetExtension(foto.FileName)
                        .ToLowerInvariant();

                    var resultado = await _imagenes.SubirAsync(
                        archivo,
                        $"foto{extension}");

                    fotosSubidas.Add(resultado);
                }

                // El vehículo y sus fotos se guardan juntos.
                await using var transaccion =
                    await _context.Database.BeginTransactionAsync();

                var vehiculo = new Vehiculo
                {
                    Marca = model.Marca.Trim(),
                    Modelo = model.Modelo.Trim(),
                    Ano = model.Ano,
                    Placa = placa,
                    Categoria = model.Categoria.Trim(),
                    Transmision = model.Transmision.Trim(),
                    PrecioPorDia = model.PrecioPorDia,
                    Estado = "Disponible"
                };

                _context.Vehiculos.Add(vehiculo);
                await _context.SaveChangesAsync();

                for (var i = 0; i < fotosSubidas.Count; i++)
                {
                    _context.VehiculoImagenes.Add(new VehiculoImagen
                    {
                        IdVehiculo = vehiculo.IdVehiculo,
                        ImagenUrl = fotosSubidas[i].Url,
                        IdPublico = fotosSubidas[i].IdPublico,
                        Orden = i + 1
                    });
                }

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Falló el registro de un vehículo con imágenes.");

                // Retiramos las fotos que llegaron a subirse.
                foreach (var foto in fotosSubidas)
                {
                    try
                    {
                        await _imagenes.EliminarAsync(foto.IdPublico);
                    }
                    catch (Exception errorLimpieza)
                    {
                        _logger.LogWarning(
                            errorLimpieza,
                            "Quedó una foto pendiente de retirar: {IdPublico}",
                            foto.IdPublico);
                    }
                }

                ModelState.AddModelError(
                    "",
                    "No se pudo completar el registro. " +
                    "Inténtalo nuevamente y vuelve a seleccionar las fotos.");

                return View(model);
            }

            TempData["Exito"] = "El vehículo se registró correctamente.";
            return RedirectToAction(nameof(Catalogo));
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
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

            return View(new VehiculoConFotosViewModel
            {
                Vehiculo = vehiculo,
                Fotos = fotos
            });
        }

        [HttpPost]
        [ActionName("Eliminar")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var vehiculo = await _context.Vehiculos
                .FirstOrDefaultAsync(v => v.IdVehiculo == id);

            if (vehiculo == null)
                return NotFound();

            var fotos = await _context.VehiculoImagenes
                .Where(i => i.IdVehiculo == id)
                .OrderBy(i => i.Orden)
                .ToListAsync();

            var model = new VehiculoConFotosViewModel
            {
                Vehiculo = vehiculo,
                Fotos = fotos.Select(i => i.ImagenUrl).ToList()
            };

            // Consulta parametrizada: comprueba las dos tablas relacionadas.
            var tieneHistorial = await _context.Database.SqlQuery<int>($"""
        SELECT CASE
            WHEN EXISTS (
                SELECT 1
                FROM dbo.Reserva
                WHERE IdVehiculo = {id}
            )
            OR EXISTS (
                SELECT 1
                FROM dbo.AlquilerContrato
                WHERE IdVehiculo = {id}
            )
            THEN 1
            ELSE 0
        END AS [Value]
        """).SingleAsync();

            if (tieneHistorial == 1)
            {
                ModelState.AddModelError(
                    "",
                    "No puedes eliminar este vehículo porque tiene reservas "
                    + "o contratos de alquiler asociados. "
                    + "Puedes marcarlo como No disponible desde Editar.");

                return View("Eliminar", model);
            }

            try
            {
                _context.VehiculoImagenes.RemoveRange(fotos);
                _context.Vehiculos.Remove(vehiculo);

                // EF guarda ambas eliminaciones en una misma transacción.
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (
                ex.GetBaseException() is Microsoft.Data.SqlClient.SqlException sql
                && sql.Number == 547)
            {
                // Protege también si se crea una reserva durante la eliminación.
                ModelState.AddModelError(
                    "",
                    "No se pudo eliminar el vehículo porque tiene "
                    + "registros relacionados. Recarga la página.");

                return View("Eliminar", model);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Falló la eliminación del vehículo {IdVehiculo}.",
                    id);

                ModelState.AddModelError(
                    "",
                    "No se pudo eliminar el vehículo. Inténtalo nuevamente.");

                return View("Eliminar", model);
            }

            // Solo retiramos archivos cuando la eliminación en la BD terminó.
            var limpiezaPendiente = false;

            foreach (var foto in fotos)
            {
                var idPublico = foto.IdPublico;

                if (string.IsNullOrWhiteSpace(idPublico))
                {
                    limpiezaPendiente = true;

                    _logger.LogWarning(
                        "La foto {IdImagen} no tiene identificador de Cloudinary.",
                        foto.IdVehiculoImagen);

                    continue;
                }

                try
                {
                    await _imagenes.EliminarAsync(idPublico);
                }
                catch (Exception ex)
                {
                    limpiezaPendiente = true;

                    _logger.LogWarning(
                        ex,
                        "Quedó pendiente retirar de Cloudinary la foto {IdPublico}.",
                        idPublico);
                }
            }

            TempData["Exito"] = limpiezaPendiente
                ? "El vehículo fue eliminado. Algunas fotos requieren "
                  + "revisar su eliminación en Cloudinary."
                : "El vehículo y sus fotos se eliminaron correctamente.";

            return RedirectToAction(nameof(Catalogo));
        }

        private static async Task<bool> EsFotoValidaAsync(IFormFile foto)
        {
            if (foto.Length < 8 || foto.Length > 5 * 1024 * 1024)
                return false;

            var extension = Path.GetExtension(foto.FileName)
                .ToLowerInvariant();

            using var archivo = foto.OpenReadStream();

            var cabecera = new byte[8];
            await archivo.ReadExactlyAsync(cabecera.AsMemory());

            var esJpg =
                cabecera[0] == 0xFF &&
                cabecera[1] == 0xD8 &&
                cabecera[2] == 0xFF;

            var esPng = cabecera.SequenceEqual(new byte[]
            {
                0x89, 0x50, 0x4E, 0x47,
                0x0D, 0x0A, 0x1A, 0x0A
            });

            return ((extension == ".jpg" || extension == ".jpeg") && esJpg)
                || (extension == ".png" && esPng);



        }
    }
}