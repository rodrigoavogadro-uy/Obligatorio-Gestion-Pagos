using Dominio.Entidades;
using Dominio.Enumeraciones;
using Microsoft.AspNetCore.Mvc;
using Web.Filters;

namespace Web.Controllers
{
    public class EmpleadoController : Controller
    {
        // Obtener el usuario logueado desde la sesión
        private Usuario ObtenerUsuarioLogueado()
        {
            string email = HttpContext.Session.GetString("UsuarioEmail");
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            // Buscar el usuario en el sistema
            List<Usuario> usuarios = Sistema.Instancia.ObtenerTodosLosUsuarios();
            foreach (Usuario u in usuarios)
            {
                if (u.Email == email)
                {
                    return u;
                }
            }
            return null;
        }

        // GET: Empleado/MisPagos
        [AutorizarRol(Rol.EMPLEADO, Rol.GERENTE)]
        public IActionResult MisPagos()
        {
            Usuario usuario = ObtenerUsuarioLogueado();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Obtener pagos del mes del usuario
            List<Pago> pagos = Sistema.Instancia.ObtenerPagosDelMesPorUsuario(usuario);

            // Ordenar por monto total descendente 
            OrdenarPagosPorMontoDescendente(pagos);

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View(pagos);
        }


        // POST: Empleado/CargarPago
        [HttpPost]
        [AutorizarRol(Rol.EMPLEADO, Rol.GERENTE)]
        public IActionResult CargarPago(string tipoPago, int idTipoGasto, string metodoPago,
                                        string descripcion, decimal monto, string fechaPago,
                                        string numeroRecibo, string fechaDesde, string fechaHasta,
                                        int cuotasPagadas)
        {
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();

            // Validaciones
            if (string.IsNullOrEmpty(tipoPago))
            {
                ViewBag.Error = "Debe seleccionar un tipo de pago.";
                return View();
            }

            if (idTipoGasto <= 0)
            {
                ViewBag.Error = "Debe seleccionar un tipo de gasto.";
                return View();
            }

            if (string.IsNullOrEmpty(metodoPago))
            {
                ViewBag.Error = "Debe seleccionar un método de pago.";
                return View();
            }

            if (string.IsNullOrEmpty(descripcion))
            {
                ViewBag.Error = "La descripción no puede estar vacía.";
                return View();
            }

            if (monto <= 0)
            {
                ViewBag.Error = "El monto debe ser mayor a cero.";
                return View();
            }

            try
            {
                Usuario usuario = ObtenerUsuarioLogueado();
                if (usuario == null)
                {
                    return RedirectToAction("Index", "Login");
                }

                // Buscar el tipo de gasto
                List<TipoGasto> tiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();
                TipoGasto tipoGastoSeleccionado = null;
                foreach (TipoGasto tg in tiposGasto)
                {
                    if (tiposGasto.IndexOf(tg) == idTipoGasto)
                    {
                        tipoGastoSeleccionado = tg;
                        break;
                    }
                }

                if (tipoGastoSeleccionado == null)
                {
                    ViewBag.Error = "Tipo de gasto no encontrado.";
                    return View();
                }

                // Convertir método de pago
                MetodoPago metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), metodoPago);

                // Crear el pago según el tipo
                if (tipoPago == "Unico")
                {
                    if (string.IsNullOrEmpty(fechaPago))
                    {
                        ViewBag.Error = "La fecha de pago no puede estar vacía.";
                        return View();
                    }

                    if (string.IsNullOrEmpty(numeroRecibo))
                    {
                        ViewBag.Error = "El número de recibo no puede estar vacío.";
                        return View();
                    }

                    DateTime fecha = DateTime.Parse(fechaPago);
                    PagoUnico pagoUnico = new PagoUnico(metodo, tipoGastoSeleccionado, usuario,
                                                        descripcion, monto, fecha, numeroRecibo);
                    Sistema.Instancia.AgregarPago(pagoUnico);
                }
                else if (tipoPago == "Recurrente")
                {
                    if (string.IsNullOrEmpty(fechaDesde))
                    {
                        ViewBag.Error = "La fecha desde no puede estar vacía.";
                        return View();
                    }

                    DateTime fDesde = DateTime.Parse(fechaDesde);
                    DateTime? fHasta = null;

                    if (!string.IsNullOrEmpty(fechaHasta))
                    {
                        fHasta = DateTime.Parse(fechaHasta);
                    }

                    PagoRecurrente pagoRecurrente = new PagoRecurrente(metodo, tipoGastoSeleccionado,
                                                                       usuario, descripcion, monto,
                                                                       fDesde, fHasta, cuotasPagadas);
                    Sistema.Instancia.AgregarPago(pagoRecurrente);
                }

                ViewBag.Exito = "Pago cargado exitosamente.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error al cargar el pago: {ex.Message}";
                return View();
            }
        }

        // GET: Empleado/Perfil
        [AutorizarRol(Rol.EMPLEADO, Rol.GERENTE)]
        public IActionResult Perfil()
        {
            Usuario usuario = ObtenerUsuarioLogueado();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.GastoMensual = Sistema.Instancia.CalcularGastoMensualUsuario(usuario);
            return View(usuario);
        }

        // Método auxiliar para ordenar pagos por monto descendente 
        private void OrdenarPagosPorMontoDescendente(List<Pago> pagos)
        {
            // Bubble sort descendente por monto
            for (int i = 0; i < pagos.Count - 1; i++)
            {
                for (int j = 0; j < pagos.Count - i - 1; j++)
                {
                    decimal monto1 = pagos[j] is PagoRecurrente ? pagos[j].Monto : pagos[j].Monto;
                    decimal monto2 = pagos[j + 1] is PagoRecurrente ? pagos[j + 1].Monto : pagos[j + 1].Monto;

                    if (monto1 < monto2)
                    {
                        // Intercambiar
                        Pago temp = pagos[j];
                        pagos[j] = pagos[j + 1];
                        pagos[j + 1] = temp;
                    }
                }
            }
        }
    }
}