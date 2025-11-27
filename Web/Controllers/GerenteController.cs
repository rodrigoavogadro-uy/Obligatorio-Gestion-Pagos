using Dominio.Entidades;
using Dominio.Enumeraciones;
using Microsoft.AspNetCore.Mvc;
using Web.Filters;

namespace Web.Controllers
{
    public class GerenteController : Controller
    {
        // Obtener el usuario logueado desde la sesión
        private Usuario ObtenerUsuarioLogueado()
        {
            string email = HttpContext.Session.GetString("UsuarioEmail");
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

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

        // GET: Gerente/PagosEquipo
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult PagosEquipo(int? mes, int? año)
        {
            Usuario gerente = ObtenerUsuarioLogueado();
            if (gerente == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Si no se especifica mes/año, usar el actual
            int mesSeleccionado = mes ?? DateTime.Now.Month;
            int añoSeleccionado = año ?? DateTime.Now.Year;

            // Obtener pagos del equipo
            List<Pago> pagos = Sistema.Instancia.ObtenerPagosDelEquipoPorMes(gerente.Equipo, mesSeleccionado, añoSeleccionado);

            // Ordenar por monto descendente 
            OrdenarPagosPorMontoDescendente(pagos);

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.MesSeleccionado = mesSeleccionado;
            ViewBag.AñoSeleccionado = añoSeleccionado;
            ViewBag.NombreEquipo = gerente.Equipo.Nombre;

            return View(pagos);
        }

        // GET: Gerente/Perfil
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult Perfil()
        {
            Usuario gerente = ObtenerUsuarioLogueado();
            if (gerente == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.GastoMensual = Sistema.Instancia.CalcularGastoMensualUsuario(gerente);

            // Obtener miembros del equipo ordenados por email
            ViewBag.MiembrosEquipo = Sistema.Instancia.ObtenerMiembrosEquipoOrdenados(gerente.Equipo);

            return View(gerente);
        }


        // POST: Gerente/CargarTipoGasto
        [HttpPost]
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult CargarTipoGasto(string nombre, string descripcion)
        {
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");

            // Validaciones
            if (string.IsNullOrEmpty(nombre))
            {
                ViewBag.Error = "El nombre no puede estar vacío.";
                return View();
            }

            if (string.IsNullOrEmpty(descripcion))
            {
                ViewBag.Error = "La descripción no puede estar vacía.";
                return View();
            }

            try
            {
                TipoGasto nuevoTipo = new TipoGasto(nombre, descripcion);
                Sistema.Instancia.AgregarTipoGasto(nuevoTipo);
                ViewBag.Exito = "Tipo de gasto creado exitosamente.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View();
            }
        }

        // GET: Gerente/EliminarTipoGasto
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult EliminarTipoGasto()
        {
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();
            return View();
        }

        // POST: Gerente/EliminarTipoGasto
        [HttpPost]
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult EliminarTipoGasto(int idTipoGasto)
        {
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();

            if (idTipoGasto < 0)
            {
                ViewBag.Error = "Debe seleccionar un tipo de gasto.";
                return View();
            }

            try
            {
                List<TipoGasto> tiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();

                if (idTipoGasto >= tiposGasto.Count)
                {
                    ViewBag.Error = "Tipo de gasto no encontrado.";
                    return View();
                }

                TipoGasto tipoAEliminar = tiposGasto[idTipoGasto];

                bool eliminado = Sistema.Instancia.EliminarTipoGasto(tipoAEliminar);

                if (eliminado)
                {
                    ViewBag.Exito = $"Tipo de gasto '{tipoAEliminar.Nombre}' eliminado exitosamente.";
                    ViewBag.TiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto(); // Actualizar lista
                }
                else
                {
                    ViewBag.Error = $"No se puede eliminar '{tipoAEliminar.Nombre}' porque está siendo utilizado por uno o más pagos.";
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error: {ex.Message}";
                return View();
            }
        }

        // GET: Gerente/CargarPago
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult CargarPago()
        {
            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            ViewBag.TiposGasto = Sistema.Instancia.ObtenerTodosTiposGasto();
            return View();
        }

        // POST: Gerente/CargarPago 
        [HttpPost]
        [AutorizarRol(Rol.GERENTE)]
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

                MetodoPago metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), metodoPago);

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

        // GET: Gerente/MisPagos
        [AutorizarRol(Rol.GERENTE)]
        public IActionResult MisPagos()
        {
            Usuario usuario = ObtenerUsuarioLogueado();
            if (usuario == null)
            {
                return RedirectToAction("Index", "Login");
            }

            List<Pago> pagos = Sistema.Instancia.ObtenerPagosDelMesPorUsuario(usuario);
            OrdenarPagosPorMontoDescendente(pagos);

            ViewBag.UsuarioNombre = HttpContext.Session.GetString("UsuarioNombre");
            return View(pagos);
        }

        // Método auxiliar para ordenar pagos
        private void OrdenarPagosPorMontoDescendente(List<Pago> pagos)
        {
            for (int i = 0; i < pagos.Count - 1; i++)
            {
                for (int j = 0; j < pagos.Count - i - 1; j++)
                {
                    decimal monto1 = pagos[j].Monto;
                    decimal monto2 = pagos[j + 1].Monto;

                    if (monto1 < monto2)
                    {
                        Pago temp = pagos[j];
                        pagos[j] = pagos[j + 1];
                        pagos[j + 1] = temp;
                    }
                }
            }
        }
    }
}