using Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    public class Sistema
    {
        // Singleton: instancia única
        private static Sistema _instancia;

        // Listas para almacenar las entidades
        private List<Equipo> _equipos;
        private List<Usuario> _usuarios;
        private List<TipoGasto> _tiposGasto;
        private List<Pago> _pagos;

        // Contadores para IDs autoincrementales
        private int _contadorEquipos;
        private int _contadorPagos;

        // Constructor privado (Singleton)
        private Sistema()
        {
            _equipos = new List<Equipo>();
            _usuarios = new List<Usuario>();
            _tiposGasto = new List<TipoGasto>();
            _pagos = new List<Pago>();
            _contadorEquipos = 1;
            _contadorPagos = 1;
        }

        // Propiedad para obtener la instancia única
        public static Sistema Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new Sistema();
                }
                return _instancia;
            }
        }

        // Agregar un equipo al sistema
        public void AgregarEquipo(Equipo equipo)
        {
            if (equipo == null)
            {
                throw new Exception("El equipo no puede ser nulo");
            }

            equipo.Validar();
            equipo.Id = _contadorEquipos;
            _contadorEquipos++;
            _equipos.Add(equipo);
        }

        // Agregar un usuario al sistema (genera email automáticamente)
        public void AgregarUsuario(Usuario usuario)
        {
            if (usuario == null)
            {
                throw new Exception("El usuario no puede ser nulo");
            }

            usuario.Validar();

            // Generar email único
            string emailGenerado = Usuario.GenerarEmail(usuario.Nombre, usuario.Apellido, _usuarios);
            usuario.Email = emailGenerado;

            _usuarios.Add(usuario);
        }

        // Agregar un tipo de gasto al sistema
        public void AgregarTipoGasto(TipoGasto tipoGasto)
        {
            if (tipoGasto == null)
            {
                throw new Exception("El tipo de gasto no puede ser nulo");
            }

            tipoGasto.Validar();
            _tiposGasto.Add(tipoGasto);
        }

        // Agregar un pago al sistema
        public void AgregarPago(Pago pago)
        {
            if (pago == null)
            {
                throw new Exception("El pago no puede ser nulo");
            }

            pago.Validar();
            pago.Id = _contadorPagos;
            _contadorPagos++;
            _pagos.Add(pago);
        }

        // Obtener todos los usuarios
        public List<Usuario> ObtenerTodosLosUsuarios()
        {
            return _usuarios;
        }

        // Obtener todos los pagos de un usuario por email
        public List<Pago> ObtenerPagosPorEmail(string email)
        {
            List<Pago> pagosUsuario = new List<Pago>();

            foreach (Pago pago in _pagos)
            {
                if (pago.Usuario.Email == email)
                {
                    pagosUsuario.Add(pago);
                }
            }

            return pagosUsuario;
        }

        // Obtener usuarios de un equipo por nombre de equipo
        public List<Usuario> ObtenerUsuariosPorEquipo(string nombreEquipo)
        {
            List<Usuario> usuariosDelEquipo = new List<Usuario>();

            foreach (Usuario usuario in _usuarios)
            {
                if (usuario.Equipo.Nombre == nombreEquipo)
                {
                    usuariosDelEquipo.Add(usuario);
                }
            }

            return usuariosDelEquipo;
        }

        // Crear un usuario con generación automática de email
        public Usuario CrearUsuario(string nombre, string apellido, string contraseña,
                                    string nombreEquipo, DateTime fechaIncorporacion, Rol rol)
        {
            // Buscar el equipo por nombre
            Equipo equipo = null;
            foreach (Equipo e in _equipos)
            {
                if (e.Nombre == nombreEquipo)
                {
                    equipo = e;
                    break;
                }
            }

            if (equipo == null)
            {
                throw new Exception($"No existe un equipo con el nombre '{nombreEquipo}'");
            }

            // Crear el usuario con el rol
            Usuario nuevoUsuario = new Usuario(nombre, apellido, contraseña, equipo, fechaIncorporacion, rol);

            // Agregar al sistema (genera el email automáticamente)
            AgregarUsuario(nuevoUsuario);

            return nuevoUsuario;
        }

        // Obtener pagos del mes actual
        public List<Pago> ObtenerPagosDelMesActual()
        {
            List<Pago> pagosDelMes = new List<Pago>();
            DateTime fechaActual = DateTime.Now;
            int mesActual = fechaActual.Month;
            int añoActual = fechaActual.Year;

            foreach (Pago pago in _pagos)
            {
                // Si es PagoUnico: verificar que la fecha de pago sea del mes actual
                if (pago is PagoUnico)
                {
                    PagoUnico pagoUnico = (PagoUnico)pago;
                    if (pagoUnico.FechaPago.Month == mesActual && pagoUnico.FechaPago.Year == añoActual)
                    {
                        pagosDelMes.Add(pago);
                    }
                }
                // Si es PagoRecurrente: verificar que esté activo en el mes actual
                else if (pago is PagoRecurrente)
                {
                    PagoRecurrente pagoRecurrente = (PagoRecurrente)pago;

                    // Fecha de inicio debe ser anterior o igual al mes actual
                    bool inicioValido = (pagoRecurrente.FechaDesde.Year < añoActual) ||
                                       (pagoRecurrente.FechaDesde.Year == añoActual &&
                                        pagoRecurrente.FechaDesde.Month <= mesActual);

                    // Si NO tiene límite (fechaHasta es null), siempre es válido el fin
                    bool finValido = false;
                    if (pagoRecurrente.FechaHasta == null)
                    {
                        finValido = true; // Sin límite, siempre activo
                    }
                    else
                    {
                        // Con límite: fecha de fin debe ser posterior o igual al mes actual
                        finValido = (pagoRecurrente.FechaHasta.Value.Year > añoActual) ||
                                   (pagoRecurrente.FechaHasta.Value.Year == añoActual &&
                                    pagoRecurrente.FechaHasta.Value.Month >= mesActual);
                    }

                    // Si ambas condiciones se cumplen, el pago está activo este mes
                    if (inicioValido && finValido)
                    {
                        pagosDelMes.Add(pago);
                    }
                }
            }

            return pagosDelMes;
        }

        // Validar si un email ya existe
        public bool ValidarEmailUnico(string email)
        {
            foreach (Usuario usuario in _usuarios)
            {
                if (usuario.Email == email)
                {
                    return false; // El email ya existe
                }
            }
            return true; // El email es único
        }


        // Validar login: retorna el usuario si email y contraseña son correctos
        public Usuario ValidarLogin(string email, string contraseña)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(contraseña))
            {
                return null;
            }

            // Buscar usuario por email y contraseña
            foreach (Usuario usuario in _usuarios)
            {
                if (usuario.Email == email && usuario.Contraseña == contraseña)
                {
                    return usuario; // Login exitoso
                }
            }

            return null; // Usuario no encontrado o contraseña incorrecta
        }


        // Obtener todos los pagos del mes actual de un usuario específico
        public List<Pago> ObtenerPagosDelMesPorUsuario(Usuario usuario)
        {
            List<Pago> pagosDelMes = new List<Pago>();
            DateTime fechaActual = DateTime.Now;
            int mesActual = fechaActual.Month;
            int añoActual = fechaActual.Year;

            foreach (Pago pago in _pagos)
            {
                // Solo pagos de este usuario
                if (pago.Usuario.Email != usuario.Email)
                {
                    continue;
                }

                // Verificar si el pago es del mes actual
                if (pago is PagoUnico)
                {
                    PagoUnico pagoUnico = (PagoUnico)pago;
                    if (pagoUnico.FechaPago.Month == mesActual && pagoUnico.FechaPago.Year == añoActual)
                    {
                        pagosDelMes.Add(pago);
                    }
                }
                else if (pago is PagoRecurrente)
                {
                    PagoRecurrente pagoRecurrente = (PagoRecurrente)pago;

                    // Fecha desde debe ser anterior o igual al mes actual
                    bool inicioValido = (pagoRecurrente.FechaDesde.Year < añoActual) ||
                                       (pagoRecurrente.FechaDesde.Year == añoActual &&
                                        pagoRecurrente.FechaDesde.Month <= mesActual);

                    bool finValido = false;
                    if (pagoRecurrente.FechaHasta == null)
                    {
                        finValido = true;
                    }
                    else
                    {
                        finValido = (pagoRecurrente.FechaHasta.Value.Year > añoActual) ||
                                   (pagoRecurrente.FechaHasta.Value.Year == añoActual &&
                                    pagoRecurrente.FechaHasta.Value.Month >= mesActual);
                    }

                    if (inicioValido && finValido)
                    {
                        pagosDelMes.Add(pago);
                    }
                }
            }

            return pagosDelMes;
        }

        // Obtener pagos de un equipo en un mes/año específico
        public List<Pago> ObtenerPagosDelEquipoPorMes(Equipo equipo, int mes, int año)
        {
            List<Pago> pagosDelEquipo = new List<Pago>();

            foreach (Pago pago in _pagos)
            {
                // Solo pagos de usuarios del equipo
                if (pago.Usuario.Equipo.Id != equipo.Id)
                {
                    continue;
                }

                // Verificar si el pago corresponde al mes/año indicado
                if (pago is PagoUnico)
                {
                    PagoUnico pagoUnico = (PagoUnico)pago;
                    if (pagoUnico.FechaPago.Month == mes && pagoUnico.FechaPago.Year == año)
                    {
                        pagosDelEquipo.Add(pago);
                    }
                }
                else if (pago is PagoRecurrente)
                {
                    PagoRecurrente pagoRecurrente = (PagoRecurrente)pago;

                    // Fecha desde debe ser anterior o igual al mes indicado
                    bool inicioValido = (pagoRecurrente.FechaDesde.Year < año) ||
                                       (pagoRecurrente.FechaDesde.Year == año &&
                                        pagoRecurrente.FechaDesde.Month <= mes);

                    bool finValido = false;
                    if (pagoRecurrente.FechaHasta == null)
                    {
                        finValido = true;
                    }
                    else
                    {
                        finValido = (pagoRecurrente.FechaHasta.Value.Year > año) ||
                                   (pagoRecurrente.FechaHasta.Value.Year == año &&
                                    pagoRecurrente.FechaHasta.Value.Month >= mes);
                    }

                    if (inicioValido && finValido)
                    {
                        pagosDelEquipo.Add(pago);
                    }
                }
            }

            return pagosDelEquipo;
        }


        // Calcular cuanto ha gastado un usuario este mes
        public decimal CalcularGastoMensualUsuario(Usuario usuario)
        {
            List<Pago> pagosDelMes = ObtenerPagosDelMesPorUsuario(usuario);
            decimal total = 0;

            foreach (Pago pago in pagosDelMes)
            {
                if (pago is PagoRecurrente)
                {
                    // Para recurrentes, usar solo el monto mensual (no el total)
                    total += pago.Monto;
                }
                else
                {
                    // Para únicos, usar el monto completo
                    total += pago.Monto;
                }
            }

            return total;
        }


        // Obtener todos los miembros de un equipo ordenados por email (sin LINQ)
        public List<Usuario> ObtenerMiembrosEquipoOrdenados(Equipo equipo)
        {
            List<Usuario> miembros = new List<Usuario>();

            // Filtrar usuarios del equipo
            foreach (Usuario usuario in _usuarios)
            {
                if (usuario.Equipo.Id == equipo.Id)
                {
                    miembros.Add(usuario);
                }
            }

            // Ordenar por email ascendente 
            for (int i = 0; i < miembros.Count - 1; i++)
            {
                for (int j = 0; j < miembros.Count - i - 1; j++)
                {
                    // Comparar emails
                    if (string.Compare(miembros[j].Email, miembros[j + 1].Email) > 0)
                    {
                        // Intercambiar
                        Usuario temp = miembros[j];
                        miembros[j] = miembros[j + 1];
                        miembros[j + 1] = temp;
                    }
                }
            }

            return miembros;
        }


        // Eliminar un tipo de gasto si no está siendo usado
        public bool EliminarTipoGasto(TipoGasto tipoGasto)
        {
            if (tipoGasto == null)
            {
                throw new Exception("El tipo de gasto no puede ser nulo");
            }

            // Verificar que no esté siendo usado por ningún pago
            foreach (Pago pago in _pagos)
            {
                if (pago.TipoGasto.Nombre == tipoGasto.Nombre)
                {
                    return false; // Está siendo usado, no se puede eliminar
                }
            }

            // No está siendo usado, se puede eliminar
            _tiposGasto.Remove(tipoGasto);
            return true;
        }


        // Obtener todos los tipos de gasto
        public List<TipoGasto> ObtenerTodosTiposGasto()
        {
            return _tiposGasto;
        }


        // Método para precarga de datos
        public void PrecargarDatos()
        {
            // ============================================
            // 1. PRECARGAR EQUIPOS (4 equipos)
            // ============================================
            Equipo equipo1 = new Equipo("CodeCrafters");
            AgregarEquipo(equipo1);

            Equipo equipo2 = new Equipo("ByteStorm");
            AgregarEquipo(equipo2);

            Equipo equipo3 = new Equipo("DevTitans");
            AgregarEquipo(equipo3);

            Equipo equipo4 = new Equipo("QuantumStack");
            AgregarEquipo(equipo4);

            // ============================================
            // 2. PRECARGAR TIPOS DE GASTO (10 tipos)
            // ============================================
            TipoGasto[] tiposGasto = new TipoGasto[]
            {
        new TipoGasto("Infraestructura", "Gastos de servidores, hosting, dominios y servicios en la nube"),
        new TipoGasto("Licencias", "Suscripciones a herramientas y software necesarios para el desarrollo"),
        new TipoGasto("Sueldos", "Pagos al personal interno, freelance y contratistas"),
        new TipoGasto("Capacitación", "Cursos, certificaciones y talleres para el equipo"),
        new TipoGasto("Equipamiento", "Compra y mantenimiento de hardware como PCs y periféricos"),
        new TipoGasto("Marketing", "Publicidad, campañas digitales y material promocional"),
        new TipoGasto("Oficina", "Alquiler, mobiliario y suministros de la oficina"),
        new TipoGasto("Eventos", "Participación en ferias, conferencias y networking"),
        new TipoGasto("Afters", "Salidas recreativas del equipo para fortalecer vínculos"),
        new TipoGasto("Transporte", "Traslados, viáticos y movilidad del personal")
            };

            foreach (TipoGasto tipo in tiposGasto)
            {
                AgregarTipoGasto(tipo);
            }

            // ============================================
            // 3. PRECARGAR USUARIOS (22 usuarios)
            // ============================================
            CrearUsuario("Ana", "Lopez", "clave1234", "CodeCrafters", new DateTime(2020, 2, 10), Rol.GERENTE);
            CrearUsuario("Juan", "Martinez", "passcode1", "CodeCrafters", new DateTime(2021, 7, 22), Rol.GERENTE);
            CrearUsuario("Lia", "Gomez", "secure987", "CodeCrafters", new DateTime(2022, 5, 14), Rol.EMPLEADO);
            CrearUsuario("Tom", "Li", "contrase12", "CodeCrafters", new DateTime(2023, 3, 30), Rol.EMPLEADO);
            CrearUsuario("Mia", "Z", "password8", "CodeCrafters", new DateTime(2024, 8, 5), Rol.EMPLEADO);
            CrearUsuario("Ax", "Perez", "codepass9", "CodeCrafters", new DateTime(2021, 10, 11), Rol.EMPLEADO);
            CrearUsuario("Sofia", "Ramos", "bytepass1", "ByteStorm", new DateTime(2020, 1, 19), Rol.EMPLEADO);
            CrearUsuario("Lucas", "Fernandez", "clave2020", "ByteStorm", new DateTime(2022, 4, 25), Rol.EMPLEADO);
            CrearUsuario("Ema", "Wu", "storm888", "ByteStorm", new DateTime(2023, 9, 12), Rol.EMPLEADO);
            CrearUsuario("Leo", "N", "devpass11", "ByteStorm", new DateTime(2024, 5, 4), Rol.EMPLEADO);
            CrearUsuario("Martina", "Sosa", "byte3210", "ByteStorm", new DateTime(2021, 6, 17), Rol.EMPLEADO);
            CrearUsuario("Igor", "Lara", "storm999", "ByteStorm", new DateTime(2022, 12, 9), Rol.EMPLEADO);
            CrearUsuario("Diego", "Torres", "titans123", "DevTitans", new DateTime(2020, 8, 7), Rol.EMPLEADO);
            CrearUsuario("Valentina", "Suarez", "clave4567", "DevTitans", new DateTime(2021, 3, 28), Rol.EMPLEADO);
            CrearUsuario("Max", "Qi", "titanpass", "DevTitans", new DateTime(2023, 1, 5), Rol.EMPLEADO);
            CrearUsuario("Ian", "B", "devtitans", "DevTitans", new DateTime(2024, 9, 2), Rol.EMPLEADO);
            CrearUsuario("Carla", "Mendez", "password1", "DevTitans", new DateTime(2022, 7, 14), Rol.EMPLEADO);
            CrearUsuario("Pablo", "Cruz", "titans777", "DevTitans", new DateTime(2023, 11, 23), Rol.EMPLEADO);
            CrearUsuario("Florencia", "Alvarez", "quantum88", "QuantumStack", new DateTime(2020, 12, 15), Rol.EMPLEADO);
            CrearUsuario("Nico", "Chen", "stackpass", "QuantumStack", new DateTime(2021, 9, 3), Rol.EMPLEADO);
            CrearUsuario("Jo", "Ramirez", "qstack22", "QuantumStack", new DateTime(2022, 6, 21), Rol.EMPLEADO);
            CrearUsuario("Elena", "Paz", "claveq789", "QuantumStack", new DateTime(2023, 4, 19), Rol.EMPLEADO);

            // Obtener lista de usuarios para usar en los pagos
            List<Usuario> usuarios = ObtenerTodosLosUsuarios();

            // ============================================
            // 4. PRECARGAR PAGOS ÚNICOS (17 pagos)
            // ============================================
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[2], usuarios[5], "Pago de sueldos de abril", 45000, new DateTime(2024, 4, 10), "REC-10234"));
            AgregarPago(new PagoUnico(MetodoPago.DEBITO, tiposGasto[0], usuarios[12], "Renovación de servidores", 32000, new DateTime(2024, 3, 22), "REC-10456"));
            AgregarPago(new PagoUnico(MetodoPago.EFECTIVO, tiposGasto[9], usuarios[3], "Viáticos para reunión de equipo", 1800, new DateTime(2024, 5, 3), "REC-10678"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[4], usuarios[20], "Compra de nuevas laptops", 48500, new DateTime(2024, 7, 18), "REC-10701"));
            AgregarPago(new PagoUnico(MetodoPago.DEBITO, tiposGasto[6], usuarios[0], "Pago de alquiler de oficina", 25000, new DateTime(2024, 1, 29), "REC-10845"));
            AgregarPago(new PagoUnico(MetodoPago.EFECTIVO, tiposGasto[8], usuarios[14], "Cena de equipo DevTitans", 4500, new DateTime(2024, 2, 14), "REC-10987"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[1], usuarios[7], "Licencia anual de herramientas", 27000, new DateTime(2024, 6, 7), "REC-11034"));
            AgregarPago(new PagoUnico(MetodoPago.DEBITO, tiposGasto[5], usuarios[10], "Campaña de marketing en redes", 15000, new DateTime(2024, 5, 20), "REC-11126"));
            AgregarPago(new PagoUnico(MetodoPago.EFECTIVO, tiposGasto[7], usuarios[2], "Inscripción a conferencia de software", 9800, new DateTime(2024, 4, 15), "REC-11253"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[3], usuarios[16], "Curso de capacitación en IA", 7500, new DateTime(2024, 9, 2), "REC-11364"));
            AgregarPago(new PagoUnico(MetodoPago.DEBITO, tiposGasto[0], usuarios[9], "Migración a nuevo hosting", 12200, new DateTime(2024, 3, 6), "REC-11489"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[6], usuarios[1], "Pago de servicios de limpieza", 5300, new DateTime(2024, 8, 28), "REC-11572"));
            AgregarPago(new PagoUnico(MetodoPago.EFECTIVO, tiposGasto[9], usuarios[18], "Combustible para traslado a evento", 2600, new DateTime(2024, 7, 12), "REC-11605"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[8], usuarios[13], "After mensual del equipo", 6200, new DateTime(2024, 6, 3), "REC-11794"));
            AgregarPago(new PagoUnico(MetodoPago.DEBITO, tiposGasto[2], usuarios[6], "Pago extra por proyecto finalizado", 38000, new DateTime(2024, 10, 5), "REC-11866"));
            AgregarPago(new PagoUnico(MetodoPago.CREDITO, tiposGasto[4], usuarios[21], "Compra de monitores adicionales", 21000, new DateTime(2024, 5, 25), "REC-11933"));
            AgregarPago(new PagoUnico(MetodoPago.EFECTIVO, tiposGasto[7], usuarios[4], "Tickets para evento tecnológico", 8300, new DateTime(2024, 8, 14), "REC-12011"));

            // ============================================
            // 5. PRECARGAR PAGOS RECURRENTES (25 pagos)
            // 20 NO pagados totalmente + 5 pagados totalmente
            // ============================================

            // NO pagados totalmente (20 pagos)
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[1], usuarios[0], "Licencia Figma Pro", 3200, new DateTime(2023, 3, 1), new DateTime(2024, 6, 1), 10));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[0], usuarios[4], "AWS Hosting", 12000, new DateTime(2023, 7, 15), new DateTime(2025, 7, 15), 8));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[5], usuarios[10], "Campaña Google Ads", 7500, new DateTime(2023, 10, 1), new DateTime(2024, 10, 1), 4));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[4], usuarios[2], "Compra de MacBooks", 18000, new DateTime(2024, 2, 5), new DateTime(2025, 2, 5), 2));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[6], usuarios[8], "Alquiler oficina", 16000, new DateTime(2023, 6, 1), null, 14)); // sin límite
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[8], usuarios[5], "After mensual", 3000, new DateTime(2024, 1, 15), new DateTime(2024, 10, 15), 5));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[1], usuarios[7], "Licencia IntelliJ", 2700, new DateTime(2023, 5, 10), new DateTime(2024, 5, 10), 11));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[0], usuarios[14], "Azure Cloud", 14500, new DateTime(2024, 6, 1), new DateTime(2025, 6, 1), 3));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[9], usuarios[12], "Movilidad Uber", 1800, new DateTime(2023, 8, 20), null, 20)); // sin límite
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[7], usuarios[1], "Suscripción a conferencias", 5200, new DateTime(2023, 4, 1), new DateTime(2025, 4, 1), 17));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[3], usuarios[6], "Curso IA Avanzada", 8000, new DateTime(2024, 3, 1), new DateTime(2024, 9, 1), 4));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[5], usuarios[16], "Publicidad Meta", 6000, new DateTime(2023, 11, 1), new DateTime(2024, 9, 1), 8));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[4], usuarios[11], "Compra de sillas", 5000, new DateTime(2024, 4, 10), new DateTime(2025, 2, 10), 1));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[2], usuarios[9], "Bonos por desempeño", 15000, new DateTime(2023, 2, 1), null, 25)); // sin límite
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[0], usuarios[3], "Servidores dedicados", 11000, new DateTime(2023, 12, 1), new DateTime(2025, 2, 1), 5));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[8], usuarios[18], "Eventos trimestrales", 2500, new DateTime(2024, 5, 20), new DateTime(2025, 3, 20), 2));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[6], usuarios[15], "Gastos de oficina", 7000, new DateTime(2023, 9, 1), new DateTime(2024, 9, 1), 9));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[3], usuarios[20], "Plataforma educativa", 2100, new DateTime(2024, 1, 10), new DateTime(2024, 9, 10), 6));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[7], usuarios[19], "Suscripción a eventos Tech", 3000, new DateTime(2023, 7, 5), new DateTime(2025, 1, 5), 14));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[1], usuarios[21], "Adobe Creative Cloud", 9500, new DateTime(2024, 3, 15), new DateTime(2025, 3, 15), 3));

            // TOTALMENTE pagados (5 pagos)
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[4], usuarios[13], "Compra de impresoras", 6000, new DateTime(2023, 5, 1), new DateTime(2024, 2, 1), 9));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[8], usuarios[17], "Eventos sociales", 4000, new DateTime(2023, 8, 1), new DateTime(2024, 1, 1), 5));
            AgregarPago(new PagoRecurrente(MetodoPago.CREDITO, tiposGasto[6], usuarios[2], "Reforma de oficina", 17000, new DateTime(2023, 6, 1), new DateTime(2024, 6, 1), 12));
            AgregarPago(new PagoRecurrente(MetodoPago.DEBITO, tiposGasto[0], usuarios[7], "Dominio web anual", 1300, new DateTime(2023, 1, 1), new DateTime(2023, 12, 1), 12));
            AgregarPago(new PagoRecurrente(MetodoPago.EFECTIVO, tiposGasto[5], usuarios[6], "Campaña publicitaria puntual", 5000, new DateTime(2023, 9, 1), new DateTime(2024, 2, 1), 6));
        }
    }
}