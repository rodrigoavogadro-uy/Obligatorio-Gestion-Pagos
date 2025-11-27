using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public IActionResult Index()
        {
            // Si ya está logueado, redirigir según su rol
            var emailSesion = HttpContext.Session.GetString("UsuarioEmail");
            if (!string.IsNullOrEmpty(emailSesion))
            {
                var rolSesion = HttpContext.Session.GetString("UsuarioRol");
                if (rolSesion == "GERENTE")
                {
                    return RedirectToAction("PagosEquipo", "Gerente");
                }
                else
                {
                    return RedirectToAction("MisPagos", "Empleado");
                }
            }

            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Index(string email, string contraseña)
        {
            // Validaciones en el controlador
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "El email no puede estar vacío.";
                return View();
            }

            if (string.IsNullOrEmpty(contraseña))
            {
                ViewBag.Error = "La contraseña no puede estar vacía.";
                return View();
            }

            // Validar login
            Usuario usuario = Sistema.Instancia.ValidarLogin(email, contraseña);

            if (usuario == null)
            {
                ViewBag.Error = "Email o contraseña incorrectos.";
                return View();
            }

            // Login exitoso: crear sesión
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
            HttpContext.Session.SetString("UsuarioNombre", $"{usuario.Nombre} {usuario.Apellido}");
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol.ToString());

            // Redirigir según el rol
            if (usuario.Rol == Dominio.Enumeraciones.Rol.GERENTE)
            {
                return RedirectToAction("PagosEquipo", "Gerente");
            }
            else
            {
                return RedirectToAction("MisPagos", "Empleado");
            }
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}