using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Enumeraciones;


namespace Dominio.Entidades
{
    public class PagoUnico : Pago
    {
        // Atributos privados
        private DateTime _fechaPago;
        private string _numeroRecibo;

        // Propiedades públicas
        public DateTime FechaPago
        {
            get { return _fechaPago; }
            set { _fechaPago = value; }
        }

        public string NumeroRecibo
        {
            get { return _numeroRecibo; }
            set
            {
                _numeroRecibo = value;
                ValidarNumeroRecibo();
            }
        }

        // Constructor
        public PagoUnico(MetodoPago metodoPago, TipoGasto tipoGasto, Usuario usuario,
                         string descripcion, decimal monto, DateTime fechaPago,
                         string numeroRecibo)
            : base(metodoPago, tipoGasto, usuario, descripcion, monto)
        {
            _fechaPago = fechaPago;
            _numeroRecibo = numeroRecibo;
            ValidarNumeroRecibo();
        }

        // Validar que el número de recibo no esté vacío
        private void ValidarNumeroRecibo()
        {
            if (string.IsNullOrEmpty(_numeroRecibo))
            {
                throw new Exception("El número de recibo no puede estar vacío");
            }
        }

        // Calcula monto total aplicando descuentos (10% o 20% si es efectivo)
        public override decimal CalcularTotal()
        {
            decimal descuento;

            // Si es efectivo: 20% de descuento
            if (MetodoPago == MetodoPago.EFECTIVO)
            {
                descuento = Monto * 0.20m;
            }
            else // Crédito o débito: 10% de descuento
            {
                descuento = Monto * 0.10m;
            }

            return Monto - descuento;
        }

        // Retorna información del estado del pago único
        public override string ObtenerEstado()
        {
            return $"Pago único - Recibo: {NumeroRecibo}";
        }

        public override string ToString()
        {
            return $"{base.ToString()} - Fecha: {FechaPago.ToShortDateString()} - Recibo: {NumeroRecibo}";
        }
    }
}