using System.Collections.Generic;
using System;

namespace ClinicDent.Models
{
    public class PagoViewModel
    {
        public int IdPago { get; set; }
        public int IdConsulta { get; set; }
        public string Paciente { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string MetodoPago { get; set; }
        public string TipoPago { get; set; }
        public string EstadoPago { get; set; }
        public int TotalCuotas { get; set; }
        public int CuotasPagadas { get; set; }
        public int CuotasPendientes { get; set; }
        public int CuotasVencidas { get; set; }
    }

    public class PagoCreateViewModel
    {
        public int IdConsulta { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string MetodoPago { get; set; }
        public string TipoPago { get; set; }
        public List<CuotaViewModel> Cuotas { get; set; }
    }

    public class CuotaViewModel
    {
        public DateTime FechaInicio { get; set; }
        public decimal Costo { get; set; }
    }

    public class ActualizarCuotaViewModel
    {
        public int IdCuota { get; set; }
        public string Estado { get; set; }
        public DateTime FechaPago { get; set; }
    }

   
}