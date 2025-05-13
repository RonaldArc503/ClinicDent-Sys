using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClinicDent.Models
{

    public class ConsultaConTratamientoViewModel
    {
        public int? IdCita { get; set; }
        public int IdDentista { get; set; }
        public int IdPaciente { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string Diagnostico { get; set; }
        public string Observaciones { get; set; }
        public string Recomendaciones { get; set; }
        public bool RequiereTratamiento { get; set; }
        public TratamientoViewModel Tratamiento { get; set; } // Un solo tratamiento
    }

    public class TratamientoViewModel
    {
        public int IdTipoCobro { get; set; }
        public DateTime FechaInicio { get; set; }
        public decimal Costo { get; set; }
        public int DuracionEstimada { get; set; }
        public bool Seguimiento { get; set; }
        public string DientesSeleccionados { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}