using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Enumeraciones;
using Dominio.Interfaces;

namespace Dominio.Entidades
{
    public abstract class Pago : IValidable
    {
        // Atributos privados
        private int _id;
        private MetodoPago _metodoPago;
        private TipoGasto _tipoGasto;
        private Usuario _usuario;
        private string _descripcion;
        private decimal _monto;

        // Propiedades públicas
        public int Id
        {
            get { return _id; }
            internal set { _id = value; }
        }

        public MetodoPago MetodoPago
        {
            get { return _metodoPago; }
            set
            {
                _metodoPago = value;
                Validar();
            }
        }

        public TipoGasto TipoGasto
        {
            get { return _tipoGasto; }
            set
            {
                _tipoGasto = value;
                Validar();
            }
        }

        public Usuario Usuario
        {
            get { return _usuario; }
            set
            {
                _usuario = value;
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

        public decimal Monto
        {
            get { return _monto; }
            set
            {
                _monto = value;
                Validar();
            }
        }

        // Constructor
        public Pago(MetodoPago metodoPago, TipoGasto tipoGasto, Usuario usuario,
                    string descripcion, decimal monto)
        {
            _metodoPago = metodoPago;
            _tipoGasto = tipoGasto;
            _usuario = usuario;
            _descripcion = descripcion;
            _monto = monto;
            Validar();
        }

        // Métodos abstractos que deben implementar las clases hijas
        public abstract decimal CalcularTotal();
        public abstract string ObtenerEstado();

        // Valida los atributos comunes de todos los pagos
        public void Validar()
        {
            // Validar descripción

            if (string.IsNullOrEmpty(_descripcion))
            {
                throw new Exception("La descripción del pago no puede estar vacía");
            }

            // Validar monto
            if (_monto <= 0)
            {
                throw new Exception("El monto del pago debe ser mayor a cero");
            }

            // Validar tipo de gasto
            if (_tipoGasto == null)
            {
                throw new Exception("El pago debe tener un tipo de gasto asociado");
            }

            // Validar usuario
            if (_usuario == null)
            {
                throw new Exception("El pago debe tener un usuario asociado");
            }
        }

        public override string ToString()
        {
            return $"Pago ID: {Id} - Método: {MetodoPago} - Monto: ${Monto} - Usuario: {Usuario.Nombre} {Usuario.Apellido}";
        }
    }
}
