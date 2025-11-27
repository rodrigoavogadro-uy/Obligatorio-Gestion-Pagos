using Dominio.Enumeraciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Filters
{
    public class AutorizarRolAttribute : ActionFilterAttribute
    {
        private readonly Rol[] _rolesPermitidos;

        // Constructor que acepta uno o más roles permitidos
        public AutorizarRolAttribute(params Rol[] rolesPermitidos)
        {
            _rolesPermitidos = rolesPermitidos;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Verificar si hay sesión activa
            var emailSesion = context.HttpContext.Session.GetString("UsuarioEmail");

            if (string.IsNullOrEmpty(emailSesion))
            {
                // No hay sesión, redirigir al login
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Verificar el rol del usuario
            var rolSesionStr = context.HttpContext.Session.GetString("UsuarioRol");

            if (string.IsNullOrEmpty(rolSesionStr))
            {
                // No hay rol en sesión, redirigir al login
                context.Result = new RedirectToActionResult("Index", "Login", null);
                return;
            }

            // Convertir string a enum
            Rol rolUsuario = (Rol)Enum.Parse(typeof(Rol), rolSesionStr);

            // Verificar si el rol del usuario está en los roles permitidos
            bool tienePermiso = false;
            foreach (Rol rolPermitido in _rolesPermitidos)
            {
                if (rolUsuario == rolPermitido)
                {
                    tienePermiso = true;
                    break;
                }
            }

            if (!tienePermiso)
            {
                // No tiene permiso, mostrar error 403
                context.Result = new ContentResult
                {
                    Content = "<h1>403 - Acceso Denegado</h1><p>No tienes permisos para acceder a esta página.</p>",
                    ContentType = "text/html",
                    StatusCode = 403
                };
            }

            base.OnActionExecuting(context);
        }
    }
}