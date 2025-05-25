using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClinicDent.Models
{
    public class PagoDetailsViewModel
    {
        public int IdPago { get; set; }
        [Display(Name = "Consulta")]
        public int? IdConsulta { get; set; }
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; }
        [Display(Name = "Monto Total")]
        public decimal MontoTotal { get; set; }
        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; }
        [Display(Name = "Tipo de Pago")]
        public string TipoPago { get; set; }
        [Display(Name = "Paciente")]
        public string PacienteNombre { get; set; }
        [Display(Name = "Teléfono")]
        public string PacienteTelefono { get; set; }
        [Display(Name = "Fecha de Nacimiento")]
        public DateTime? PacienteFechaNacimiento { get; set; }
        [Display(Name = "Dentista")]
        public string DentistaNombre { get; set; }
        [Display(Name = "Fecha de Consulta")]
        public DateTime? FechaConsulta { get; set; }
        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; }
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; }
        [Display(Name = "Recomendaciones")]
        public string Recomendaciones { get; set; }
        public List<PagoCuotaViewModel> Cuotas { get; set; }
        [Display(Name = "ID Paciente")]
        public int? IdPaciente { get; set; } // Added to store patient ID
    }
}