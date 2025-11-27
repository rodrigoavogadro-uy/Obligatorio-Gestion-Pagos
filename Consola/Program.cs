using Dominio.Entidades;
using Dominio.Enumeraciones;
namespace Consola
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Ejecutar precarga de datos
            Console.WriteLine("Cargando datos del sistema...");
            Sistema.Instancia.PrecargarDatos();
            Console.WriteLine("Datos cargados correctamente.\n");

            // Mostrar menú principal
            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        ListarTodosLosUsuarios();
                        break;
                    case "2":
                        ListarPagosPorEmail();
                        break;
                    case "3":
                        AltaDeUsuario();
                        break;
                    case "4":
                        ListarUsuariosPorEquipo();
                        break;
                    case "5":
                        salir = true;
                        Console.WriteLine("\n¡Hasta luego!");
                        break;
                    default:
                        Console.WriteLine("\nOpción inválida. Intente nuevamente.\n");
                        break;
                }

                if (!salir)
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        static void MostrarMenu()
        {
            Console.WriteLine("========================================");
            Console.WriteLine("   SISTEMA DE GESTIÓN DE GASTOS");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Listar todos los usuarios");
            Console.WriteLine("2. Listar pagos de un usuario (por email)");
            Console.WriteLine("3. Alta de usuario");
            Console.WriteLine("4. Listar usuarios de un equipo");
            Console.WriteLine("5. Salir");
            Console.WriteLine("========================================");
            Console.Write("Seleccione una opción: ");
        }

        // Opción 1: Listar todos los usuarios
        static void ListarTodosLosUsuarios()
        {
            Console.WriteLine("\n=== LISTADO DE TODOS LOS USUARIOS ===\n");

            List<Usuario> usuarios = Sistema.Instancia.ObtenerTodosLosUsuarios();

            if (usuarios.Count == 0)
            {
                Console.WriteLine("No hay usuarios registrados.");
                return;
            }

            Console.WriteLine($"Total de usuarios: {usuarios.Count}\n");
            Console.WriteLine("{0,-20} {1,-30} {2,-20}", "NOMBRE", "EMAIL", "EQUIPO");
            Console.WriteLine(new string('-', 70));

            foreach (Usuario usuario in usuarios)
            {
                string nombreCompleto = $"{usuario.Nombre} {usuario.Apellido}";
                Console.WriteLine("{0,-20} {1,-30} {2,-20}",
                    nombreCompleto,
                    usuario.Email,
                    usuario.Equipo.Nombre);
            }
        }

        // Opción 2: Listar pagos de un usuario por email (POLIMORFISMO)
        static void ListarPagosPorEmail()
        {
            Console.WriteLine("\n=== LISTAR PAGOS DE UN USUARIO ===\n");

            Console.Write("Ingrese el email del usuario: ");
            string email = Console.ReadLine();

            if (string.IsNullOrEmpty(email))
            {
                Console.WriteLine("\nError: El email no puede estar vacío.");
                return;
            }

            // Validar formato básico de email
            if (!email.Contains("@") || !email.Contains("."))
            {
                Console.WriteLine("\nError: El email debe tener formato válido (contener @ y .)");
                return;
            }

            //llamar a Sistema
            List<Pago> pagos = Sistema.Instancia.ObtenerPagosPorEmail(email);

            if (pagos.Count == 0)
            {
                Console.WriteLine($"\nNo se encontraron pagos para el email: {email}");
                return;
            }

            Console.WriteLine($"\nPagos realizados por {email}:");
            Console.WriteLine($"Total: {pagos.Count} pago(s)\n");

            foreach (Pago pago in pagos)
            {
                Console.WriteLine("----------------------------------------");
                Console.WriteLine($"ID: {pago.Id}");
                Console.WriteLine($"Método de pago: {pago.MetodoPago}");
                Console.WriteLine($"Tipo de gasto: {pago.TipoGasto.Nombre}");
                Console.WriteLine($"Descripción: {pago.Descripcion}");
                Console.WriteLine($"Monto base: ${pago.Monto}");

                // POLIMORFISMO: CalcularTotal() se comporta diferente según el tipo
                Console.WriteLine($"Monto total: ${pago.CalcularTotal()}");

                Console.WriteLine($"Estado: {pago.ObtenerEstado()}");

                Console.WriteLine("----------------------------------------");
            }
        }

        // Opción 3: Alta de usuario (con generación automática de email)
        static void AltaDeUsuario()
        {
            Console.WriteLine("\n=== ALTA DE USUARIO ===\n");

            // Solicitar datos
            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();

            Console.Write("Apellido: ");
            string apellido = Console.ReadLine();

            Console.Write("Contraseña (mínimo 8 caracteres): ");
            string contraseña = Console.ReadLine();

            Console.Write("Nombre del equipo: ");
            string nombreEquipo = Console.ReadLine();

            Console.Write("Fecha de incorporación (formato: dd/MM/yyyy): ");
            string fechaStr = Console.ReadLine();

            Console.Write("Rol (1: EMPLEADO, 2: GERENTE): ");
            string rolStr = Console.ReadLine();

            // VALIDACIONES EN PROGRAM ANTES DE LLAMAR A SISTEMA
            if (string.IsNullOrEmpty(nombre))
            {
                Console.WriteLine("\nError: El nombre no puede estar vacío.");
                return;
            }

            if (string.IsNullOrEmpty(apellido))
            {
                Console.WriteLine("\nError: El apellido no puede estar vacío.");
                return;
            }

            if (string.IsNullOrEmpty(contraseña))
            {
                Console.WriteLine("\nError: La contraseña no puede estar vacía.");
                return;
            }

            if (string.IsNullOrEmpty(nombreEquipo))
            {
                Console.WriteLine("\nError: El nombre del equipo no puede estar vacío.");
                return;
            }

            if (string.IsNullOrEmpty(fechaStr))
            {
                Console.WriteLine("\nError: La fecha no puede estar vacía.");
                return;
            }

            if (string.IsNullOrEmpty(rolStr))
            {
                Console.WriteLine("\nError: Debe seleccionar un rol.");
                return;
            }

            // AHORA SÍ intentar crear el usuario (validaciones de negocio en Usuario)
            try
            {
                DateTime fechaIncorporacion = DateTime.Parse(fechaStr);

                // Determinar el rol
                Rol rol;
                if (rolStr == "1")
                {
                    rol = Rol.EMPLEADO;
                }
                else if (rolStr == "2")
                {
                    rol = Rol.GERENTE;
                }
                else
                {
                    Console.WriteLine("\nError: Opción de rol inválida. Use 1 o 2.");
                    return;
                }

                Usuario nuevoUsuario = Sistema.Instancia.CrearUsuario(
                    nombre,
                    apellido,
                    contraseña,
                    nombreEquipo,
                    fechaIncorporacion,
                    rol); // ← NUEVO PARÁMETRO

                Console.WriteLine("\n✓ Usuario creado exitosamente!");
                Console.WriteLine($"  Nombre: {nuevoUsuario.Nombre} {nuevoUsuario.Apellido}");
                Console.WriteLine($"  Email generado: {nuevoUsuario.Email}");
                Console.WriteLine($"  Equipo: {nuevoUsuario.Equipo.Nombre}");
                Console.WriteLine($"  Rol: {nuevoUsuario.Rol}");
            }
            catch (FormatException)
            {
                Console.WriteLine("\nError: Formato de fecha inválido. Use dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
        }


        // Opción 4: Listar usuarios de un equipo
        static void ListarUsuariosPorEquipo()
        {
            Console.WriteLine("\n=== LISTAR USUARIOS DE UN EQUIPO ===\n");

            Console.Write("Ingrese el nombre del equipo: ");
            string nombreEquipo = Console.ReadLine();

            // VALIDACIONES EN PROGRAM
            if (string.IsNullOrEmpty(nombreEquipo))
            {
                Console.WriteLine("\nError: El nombre del equipo no puede estar vacío.");
                return;
            }

            // Validación adicional: no solo espacios
            if (nombreEquipo.Trim().Length == 0)
            {
                Console.WriteLine("\nError: El nombre del equipo no puede contener solo espacios.");
                return;
            }

            // AHORA SÍ llamar a Sistema
            List<Usuario> usuarios = Sistema.Instancia.ObtenerUsuariosPorEquipo(nombreEquipo);

            if (usuarios.Count == 0)
            {
                Console.WriteLine($"\nNo se encontraron usuarios en el equipo: {nombreEquipo}");
                return;
            }

            Console.WriteLine($"\nUsuarios del equipo '{nombreEquipo}':");
            Console.WriteLine($"Total: {usuarios.Count} usuario(s)\n");
            Console.WriteLine("{0,-25} {1,-30}", "NOMBRE", "EMAIL");
            Console.WriteLine(new string('-', 55));

            foreach (Usuario usuario in usuarios)
            {
                string nombreCompleto = $"{usuario.Nombre} {usuario.Apellido}";
                Console.WriteLine("{0,-25} {1,-30}", nombreCompleto, usuario.Email);
            }
        }
    }
}