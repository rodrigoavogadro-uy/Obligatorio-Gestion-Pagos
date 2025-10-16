using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Interfaces;

namespace Dominio.Entidades
{
    public class TipoGasto : IValidable
    {
        // Atributos privados
        private string _nombre;
        private string _descripcion;

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

        public string Descripcion
        {
            get { return _descripcion; }
            set
            {
                _descripcion = value;
                Validar();
            }
        }

        // Constructor
        public TipoGasto(string nombre, string descripcion)
        {
            _nombre = nombre;
            _descripcion = descripcion;
            Validar();
        }

        // Valida que nombre y descripción no sean nulos ni vacíos
        public void Validar()
        {
            // Validar nombre

            if (string.IsNullOrEmpty(_nombre))
            {
                throw new Exception("El nombre del tipo de gasto no puede estar vacío");
            }

            // Validar descripción


            if (string.IsNullOrEmpty(_descripcion))
            {
                throw new Exception("La descripción del tipo de gasto no puede estar vacía");
            }
        }

        public override string ToString()
        {
            return $"{Nombre}: {Descripcion}";
        }
    }
}
