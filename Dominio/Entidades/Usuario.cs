using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Interfaces;

namespace Dominio.Entidades
{
    public class Usuario : IValidable
    {
        // Atributos privados
        private string _nombre;
        private string _apellido;
        private string _contraseña;
        private string _email;
        private Equipo _equipo;
        private DateTime _fechaIncorporacion;

        // Propiedades públicas
        public string Nombre
        {
            get { return _nombre; }
            set
            {
                _nombre = value;
                Validar();
            }
        }

        public string Apellido
        {
            get { return _apellido; }
            set
            {
                _apellido = value;
                Validar();
            }
        }

        public string Contraseña
        {
            get { return _contraseña; }
            set
            {
                _contraseña = value;
                Validar();
            }
        }

        public string Email
        {
            get { return _email; }
            internal set { _email = value; } // Solo Sistema puede asignar el email
        }

        public Equipo Equipo
        {
            get { return _equipo; }
            set
            {
                _equipo = value;
                Validar();
            }
        }

        public DateTime FechaIncorporacion
        {
            get { return _fechaIncorporacion; }
            set { _fechaIncorporacion = value; }
        }

        // Constructor
        public Usuario(string nombre, string apellido, string contraseña,
                       Equipo equipo, DateTime fechaIncorporacion)
        {
            _nombre = nombre;
            _apellido = apellido;
            _contraseña = contraseña;
            _equipo = equipo;
            _fechaIncorporacion = fechaIncorporacion;
            Validar();
        }

        // Método estático para generar email automáticamente
        // Método estático para generar email automáticamente
        public static string GenerarEmail(string nombre, string apellido, List<Usuario> usuariosExistentes)
        {
            // Validar parámetros
            if (string.IsNullOrEmpty(nombre))
            {
                throw new Exception("El nombre no puede estar vacío");
            }

            if (string.IsNullOrEmpty(apellido))
            {
                throw new Exception("El apellido no puede estar vacío");
            }

            // Tomar primeras 3 letras del nombre (o completo si tiene menos)
            string parteNombre;
            if (nombre.Length >= 3)
            {
                parteNombre = nombre.Substring(0, 3).ToLower();
            }
            else
            {
                parteNombre = nombre.ToLower();
            }

            // Tomar primeras 3 letras del apellido (o completo si tiene menos)
            string parteApellido;
            if (apellido.Length >= 3)
            {
                parteApellido = apellido.Substring(0, 3).ToLower();
            }
            else
            {
                parteApellido = apellido.ToLower();
            }

            // Generar email base
            string emailBase = parteNombre + parteApellido + "@laEmpresa.com";
            string emailFinal = emailBase;

            // Verificar si el email ya existe y agregar número si es necesario
            if (usuariosExistentes != null)
            {
                int contador = 1;
                bool emailExiste = true;

                // Mientras el email exista, seguir probando con números
                while (emailExiste)
                {
                    emailExiste = false;

                    // Recorrer la lista de usuarios existentes con foreach
                    foreach (Usuario usuario in usuariosExistentes)
                    {
                        if (usuario.Email == emailFinal)
                        {
                            emailExiste = true;
                            break;
                        }
                    }

                    // Si existe, generar nuevo email con contador
                    if (emailExiste)
                    {
                        emailFinal = parteNombre + parteApellido + contador + "@laEmpresa.com";
                        contador++;
                    }
                }
            }

            return emailFinal;
        }

        // Valida todos los atributos del usuario
        public void Validar()
        {
            // Validar nombre
            if (_nombre == null)
            {
                throw new Exception("El nombre del usuario no puede ser nulo");
            }

            if (string.IsNullOrEmpty(_nombre))
            {
                throw new ArgumentException("El nombre del usuario no puede estar vacío");
            }

            // Validar apellido

            if (string.IsNullOrEmpty(_apellido))
            {
                throw new Exception("El apellido del usuario no puede estar vacío");
            }

            // Validar contraseña
            if (_contraseña == null)
            {
                throw new Exception("La contraseña no puede ser nula");
            }

            if (_contraseña.Length < 8)
            {
                throw new Exception("La contraseña debe tener al menos 8 caracteres");
            }

            // Validar equipo
            if (_equipo == null)
            {
                throw new Exception("El usuario debe pertenecer a un equipo");
            }
        }

        public override string ToString()
        {
            return $"{Nombre} {Apellido} - {Email} (Equipo: {Equipo.Nombre})";
        }
    }
}