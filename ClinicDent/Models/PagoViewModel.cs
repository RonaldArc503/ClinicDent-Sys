using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ClinicDent.Models
{
    public class PagoCreateViewModel
    {
        [Required(ErrorMessage = "Seleccione una consulta")]
        [Display(Name = "Consulta")]
        public int IdConsulta { get; set; }

        [Required(ErrorMessage = "El monto total es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor que 0")]
        [Display(Name = "Monto Total")]
        public decimal MontoTotal { get; set; }

        [Required(ErrorMessage = "Seleccione un método de pago")]
        [Display(Name = "Método de Pago")]
        public string MetodoPago { get; set; }

        [Required(ErrorMessage = "Seleccione un tipo de pago")]
        [Display(Name = "Tipo de Pago")]
        public string TipoPago { get; set; }

        [Range(0, 12, ErrorMessage = "El número de cuotas debe estar entre 0 y 12")]
        [Display(Name = "Número de Cuotas")]
        public int NumeroCuotas { get; set; }

        public List<CuotaInputModel> Cuotas { get; set; } = new List<CuotaInputModel>();

        public IEnumerable<SelectListItem> Consultas { get; set; }
        public IEnumerable<SelectListItem> MetodosPago { get; set; }
        public IEnumerable<SelectListItem> TiposPago { get; set; }
    }

    public class CuotaInputModel
    {
        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [Display(Name = "Fecha de Pago")]
        public DateTime FechaPago { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 999999.99, ErrorMessage = "El monto debe ser mayor que 0")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio")]
        [Display(Name = "Estado")]
        public string Estado { get; set; }
    }

    public class PagoViewModel
    {
        public int IdPago { get; set; }
        public int? IdConsulta { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoTotal { get; set; }
        public string MetodoPago { get; set; }
        public string TipoPago { get; set; }
        public List<PagoCuotaViewModel> Cuotas { get; set; } = new List<PagoCuotaViewModel>();
    }

    public class PagoCuotaViewModel
    {
        public int IdCuota { get; set; }
        public int IdPago { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string Estado { get; set; }
    }
}