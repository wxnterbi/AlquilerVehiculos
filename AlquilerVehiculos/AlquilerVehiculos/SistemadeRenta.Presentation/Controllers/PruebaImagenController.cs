using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemadeRenta.Presentation.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PruebaImagenController : Controller
    {
        private readonly Cloudinary _cloudinary;
        private readonly IWebHostEnvironment _entorno;

        public PruebaImagenController(
            Cloudinary cloudinary,
            IWebHostEnvironment entorno)
        {
            _cloudinary = cloudinary;
            _entorno = entorno;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!_entorno.IsDevelopment())
                return NotFound();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> Index(IFormFile? foto)
        {
            if (!_entorno.IsDevelopment())
                return NotFound();

            if (foto == null || foto.Length == 0)
            {
                ModelState.AddModelError("", "Selecciona una foto.");
                return View();
            }

            var extension = Path.GetExtension(foto.FileName)
                .ToLowerInvariant();

            if (foto.Length > 5 * 1024 * 1024 ||
                (extension != ".jpg" &&
                 extension != ".jpeg" &&
                 extension != ".png"))
            {
                ModelState.AddModelError("",
                    "Selecciona una imagen JPG o PNG de hasta 5 MB.");

                return View();
            }

            try
            {
                using var archivo = foto.OpenReadStream();

                var resultado = await _cloudinary.UploadAsync(
                    new ImageUploadParams
                    {
                        File = new FileDescription(
                            $"prueba{extension}", archivo),
                        PublicId = $"pruebas-renta/{Guid.NewGuid():N}"
                    });

                if (resultado.Error != null ||
                    resultado.SecureUrl == null)
                {
                    ModelState.AddModelError("",
                        "Cloudinary rechazó la subida. " +
                        "Revisa las claves y la configuración.");

                    return View();
                }

                ViewBag.ImagenUrl = resultado.SecureUrl.ToString();
                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError("",
                    "No se pudo completar la subida. " +
                    "Revisa la conexión y la configuración de Cloudinary.");

                return View();
            }
        }
    }
}
