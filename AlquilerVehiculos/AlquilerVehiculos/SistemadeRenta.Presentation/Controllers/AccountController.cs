using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemadeRenta.Domain.Entities;
using SistemadeRenta.Persistence.Context;
using SistemadeRenta.Presentation.Models;

namespace SistemadeRenta.Presentation.Controllers
{
    public class AccountController : Controller
    {
        private readonly SistemadeRentaContext _context;
        private readonly IPasswordHasher<Usuario> _hasher;

        public AccountController(SistemadeRentaContext context, IPasswordHasher<Usuario> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Catalogo", "Vehiculos");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var correo = model.Correo.Trim().ToLowerInvariant();
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Activo);

            // Mismo mensaje si el correo no existe o la contraseña falla (no revela cuál).
            var resultado = usuario == null
                ? PasswordVerificationResult.Failed
                : _hasher.VerifyHashedPassword(usuario, usuario.ContrasenaHash, model.Contrasena);

            if (usuario == null || resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
                model.Contrasena = string.Empty;
                return View(model);
            }

            // Si el algoritmo de hash fue actualizado, se regenera el hash.
            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.ContrasenaHash = _hasher.HashPassword(usuario, model.Contrasena);
                await _context.SaveChangesAsync();
            }

            await IniciarSesionAsync(usuario, model.Recordarme);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Catalogo", "Vehiculos");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro() => View(new RegistroViewModel());

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var correo = model.Correo.Trim().ToLowerInvariant();

            if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
            {
                ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta con ese correo.");
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre.Trim(),
                Correo = correo,
                Rol = "Cliente",
                Activo = true
            };
            usuario.ContrasenaHash = _hasher.HashPassword(usuario, model.Contrasena);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            await IniciarSesionAsync(usuario, recordarme: false);
            return RedirectToAction("Catalogo", "Vehiculos");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccesoDenegado() => View();

        private async Task IniciarSesionAsync(Usuario usuario, bool recordarme)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = recordarme });
        }
    }
}
