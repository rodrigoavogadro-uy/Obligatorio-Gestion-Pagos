using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dominio.Enumeraciones;

using Dominio.Enumeraciones;

namespace Dominio.Entidades
{
    public class PagoRecurrente : Pago
    {
        // Atributos privados
        private DateTime _fechaDesde;
        private DateTime? _fechaHasta; // Nullable: puede no tener límite
        private int _cuotasPagadas;

        // Propiedades públicas
        public DateTime FechaDesde
        {
            get { return _fechaDesde; }
            set { _fechaDesde = value; }
        }

        public DateTime? FechaHasta
        {
            get { return _fechaHasta; }
            set
            {
                _fechaHasta = value;
                ValidarFechas();
            }
        }

        public int CuotasPagadas
        {
            get { return _cuotasPagadas; }
            set
            {
                _cuotasPagadas = value;
                ValidarCuotasPagadas();
            }
        }

        // Propiedad calculada: cantidad total de cuotas
        public int CantidadCuotas
        {
            get
            {
                if (_fechaHasta == null)
                {
                    return 0; // Sin límite, no tiene cantidad definida
                }

                // Calcular cantidad de meses entre fechaDesde y fechaHasta
                int meses = ((_fechaHasta.Value.Year - _fechaDesde.Year) * 12) +
                            (_fechaHasta.Value.Month - _fechaDesde.Month) + 1;
                return meses;
            }
        }

        // Constructor
        public PagoRecurrente(MetodoPago metodoPago, TipoGasto tipoGasto, Usuario usuario,
                              string descripcion, decimal monto, DateTime fechaDesde,
                              DateTime? fechaHasta, int cuotasPagadas)
            : base(metodoPago, tipoGasto, usuario, descripcion, monto)
        {
            _fechaDesde = fechaDesde;
            _fechaHasta = fechaHasta;
            _cuotasPagadas = cuotasPagadas;
            ValidarFechas();
            ValidarCuotasPagadas();
        }

        // Validar que fechaHasta sea posterior a fechaDesde
        private void ValidarFechas()
        {
            if (_fechaHasta != null && _fechaHasta <= _fechaDesde)
            {
                throw new Exception("La fecha hasta debe ser posterior a la fecha desde");
            }
        }

        // Validar que cuotas pagadas no sean negativas
        private void ValidarCuotasPagadas()
        {
            if (_cuotasPagadas < 0)
            {
                throw new Exception("Las cuotas pagadas no pueden ser negativas");
            }

            // Si tiene límite, no puede haber pagado más cuotas que las totales
            if (_fechaHasta != null && _cuotasPagadas > CantidadCuotas)
            {
                throw new Exception("No se pueden haber pagado más cuotas que las totales");
            }
        }

        // Implementación del método abstracto: calcula monto total con recargos
        public override decimal CalcularTotal()
        {
            decimal montoBase;

            // Si tiene límite, calcular según cantidad de cuotas
            if (_fechaHasta != null)
            {
                montoBase = Monto * CantidadCuotas;
            }
            else
            {
                // Sin límite: usar el monto mensual
                montoBase = Monto;
            }

            // Aplicar recargos según la letra (lo veremos en la siguiente parte)
            decimal recargo = CalcularRecargo();
            return montoBase + recargo;
        }

        // Calcula el recargo según cantidad de cuotas
        private decimal CalcularRecargo()
        {
            // Sin límite: 3% de recargo
            if (_fechaHasta == null)
            {
                return Monto * 0.03m;
            }

            // Con límite: recargo según cantidad de cuotas
            int cuotas = CantidadCuotas;
            decimal montoTotal = Monto * cuotas;

            if (cuotas > 10)
            {
                return montoTotal * 0.10m; // 10% de recargo
            }
            else if (cuotas >= 6 && cuotas <= 9)
            {
                return montoTotal * 0.05m; // 5% de recargo
            }
            else
            {
                return montoTotal * 0.03m; // 3% de recargo (5 o menos cuotas)
            }
        }

        // Implementación del método abstracto: retorna estado del pago
        public override string ObtenerEstado()
        {
            // Sin límite
            if (_fechaHasta == null)
            {
                return "recurrente";
            }

            // Con límite: calcular cuotas pendientes
            int cuotasPendientes = CantidadCuotas - _cuotasPagadas;

            if (cuotasPendientes <= 0)
            {
                return "pagado completamente";
            }

            return $"{cuotasPendientes} cuotas pendientes";
        }

        public override string ToString()
        {
            return $"{base.ToString()} - Recurrente - Estado: {ObtenerEstado()}";
        }
    }
}