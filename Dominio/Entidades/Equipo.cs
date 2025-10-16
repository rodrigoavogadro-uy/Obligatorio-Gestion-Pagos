using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Interfaces;

namespace Dominio.Entidades
{
    public class Equipo : IValidable
    {
        // Atributos privados
        private int _id;
        private string _nombre;

        // Propiedades públicas
        public int Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        public string Nombre
        {
            get { return _nombre; }
            set
            {
                _nombre = value;
                Validar(); // Se valida automáticamente al cambiar
            }
        }

        // Constructor
        public Equipo(string nombre)
        {
            _nombre = nombre;
            Validar();
        }
        // Recibe solo el nombre (el ID se asigna después desde Sistema)
        // Valida que el nombre no sea nulo ni vacío
        public void Validar()
        {

            if (string.IsNullOrEmpty(_nombre))
            {
                throw new Exception("El nombre del equipo no puede estar vacío ni ser nulo");
            }
        }

        public override string ToString()
        {
            return $"Equipo: {Nombre} (ID: {Id})";
        }
    }
}