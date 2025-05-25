using System;
using System.Linq;
using System.Web.Mvc;
using ClinicDent.Models;
using System.Data.Entity;

namespace ClinicDent.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        public ActionResult Index()
        {
            // Calcular la fecha límite fuera de la consulta LINQ
            DateTime fechaLimite = DateTime.Today.AddDays(-7);

            // Obtener citas recientes (últimos 7 días) con Pacientes y Dentistas no nulos
            var citas = db.Citas
                .Include(c => c.Dentistas)
                .Include(c => c.Pacientes)
                .Where(c => c.fecha_hora >= fechaLimite)
                .Where(c => c.Pacientes != null && c.Dentistas != null) // Filtrar citas con relaciones válidas
                .OrderBy(c => c.fecha_hora)
                .ToList();

            // Depuración: Imprimir en consola para verificar datos
            foreach (var cita in citas)
            {
                System.Diagnostics.Debug.WriteLine($"Cita ID: {cita.id_cita}, Paciente: {cita.Pacientes?.nombres ?? "Nulo"}, Dentista: {cita.Dentistas?.nombre ?? "Nulo"}");
            }

            // Configurar ViewBag para el formulario de búsqueda
            ViewBag.Estados = new string[] { "Confirmada", "Pendiente", "Cancelada", "Completada" };
            ViewBag.CurrentFilter = "";
            ViewBag.FilterBy = "ambos";
            ViewBag.EstadoFilter = "Todos";
            ViewBag.FechaFilter = "";

            return View(citas);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}