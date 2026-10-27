using Microsoft.AspNetCore.Mvc;

namespace AlquilerVehiculos.Presentation.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string correo, string contrasena)
        {
            if (correo == "admin@admin.com" && contrasena == "123456")
            {
                return RedirectToAction("Catalogo", "Vehiculos");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }
    }
}
