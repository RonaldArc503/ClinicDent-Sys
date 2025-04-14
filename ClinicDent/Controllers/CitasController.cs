using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClinicDent.Models;

namespace ClinicDent.Controllers
{
    public class CitasController : Controller
    {
        private ClinicaDentalLocal db = new ClinicaDentalLocal();

        // GET: Citas
        public ActionResult Index(string searchString, string filterBy, string estadoFilter, string fechaFilter)
        {
            var citas = db.Citas.Include(c => c.Dentistas).Include(c => c.Pacientes).AsQueryable();

            // Filtro por texto de búsqueda
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                switch (filterBy)
                {
                    case "paciente":
                        citas = citas.Where(c => c.Pacientes.nombres.ToLower().Contains(searchString));
                        break;
                    case "dentista":
                        citas = citas.Where(c => c.Dentistas.nombre.ToLower().Contains(searchString));
                        break;
                    case "ambos":
                        citas = citas.Where(c =>
                            c.Pacientes.nombres.ToLower().Contains(searchString) ||
                            c.Dentistas.nombre.ToLower().Contains(searchString));
                        break;
                    default:
                        citas = citas.Where(c =>
                            c.Pacientes.nombres.ToLower().Contains(searchString) ||
                            c.Dentistas.nombre.ToLower().Contains(searchString) ||
                            c.estado.ToLower().Contains(searchString));
                        break;
                }
            }

            // Filtro por estado
            if (!String.IsNullOrEmpty(estadoFilter) && estadoFilter != "Todos")
            {
                citas = citas.Where(c => c.estado == estadoFilter);
            }

            // Filtro por fecha
            if (!String.IsNullOrEmpty(fechaFilter))
            {
                switch (fechaFilter)
                {
                    case "hoy":
                        citas = citas.Where(c => DbFunctions.TruncateTime(c.fecha_hora) == DbFunctions.TruncateTime(DateTime.Today));
                        break;
                    case "semana":
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(7);
                        citas = citas.Where(c => c.fecha_hora >= startOfWeek && c.fecha_hora < endOfWeek);
                        break;
                    case "mes":
                        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1);
                        citas = citas.Where(c => c.fecha_hora >= startOfMonth && c.fecha_hora < endOfMonth);
                        break;
                    case "pasadas":
                        citas = citas.Where(c => DbFunctions.TruncateTime(c.fecha_hora) < DbFunctions.TruncateTime(DateTime.Today));
                        break;
                    case "futuras":
                        citas = citas.Where(c => DbFunctions.TruncateTime(c.fecha_hora) >= DbFunctions.TruncateTime(DateTime.Today));
                        break;
                }
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.EstadoFilter = estadoFilter;
            ViewBag.FechaFilter = fechaFilter;

            ViewBag.Estados = new List<string> { "Confirmada", "Pendiente", "Cancelada", "Completada" };

            return View(citas.OrderBy(c => c.fecha_hora).ToList());
        }

        // GET: Citas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Citas citas = db.Citas.Find(id);
            if (citas == null)
            {
                return HttpNotFound();
            }
            return View(citas);
        }

        // GET: Citas/Create
        public ActionResult Create()
        {
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre");
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres");
            return View();
        }

        // POST: Citas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_cita,id_paciente,id_dentista,fecha_hora,estado")] Citas citas)
        {
            bool citaExiste = db.Citas.Any(c =>
                c.id_dentista == citas.id_dentista &&
                DbFunctions.TruncateTime(c.fecha_hora) == citas.fecha_hora.Date &&
                c.fecha_hora.Hour == citas.fecha_hora.Hour &&
                c.fecha_hora.Minute == citas.fecha_hora.Minute
            );

            if (citaExiste)
            {
                ModelState.AddModelError("", "Ya existe una cita con este dentista a la misma fecha y hora.");
            }

            if (ModelState.IsValid)
            {
                db.Citas.Add(citas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", citas.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", citas.id_paciente);
            return View(citas);
        }

        // GET: Citas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Citas citas = db.Citas.Find(id);
            if (citas == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", citas.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", citas.id_paciente);
            return View(citas);
        }

        // POST: Citas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_cita,id_paciente,id_dentista,fecha_hora,estado")] Citas citas)
        {
            bool citaOcupada = db.Citas.Any(c =>
                c.id_cita != citas.id_cita &&
                c.id_dentista == citas.id_dentista &&
                DbFunctions.TruncateTime(c.fecha_hora) == citas.fecha_hora.Date &&
                c.fecha_hora.Hour == citas.fecha_hora.Hour &&
                c.fecha_hora.Minute == citas.fecha_hora.Minute
            );

            if (citaOcupada)
            {
                ModelState.AddModelError("", "Ya existe otra cita con este dentista a la misma fecha y hora.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(citas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", citas.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", citas.id_paciente);
            return View(citas);
        }

        // GET: Citas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Citas citas = db.Citas.Find(id);
            if (citas == null)
            {
                return HttpNotFound();
            }
            return View(citas);
        }

        // POST: Citas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Citas citas = db.Citas.Find(id);
            db.Citas.Remove(citas);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Calendario()
        {
            return View();
        }

        // En CitasController.cs

        [HttpGet]
        public JsonResult ObtenerEventos()
        {
            try
            {
                var citas = db.Citas
                    .Include(c => c.Pacientes)
                    .Include(c => c.Dentistas)
                    .Where(c => c.fecha_hora != null)
                    .ToList();

                var eventos = citas.Select(c => new
                {
                    id = c.id_cita,
                    title = $"{c.Pacientes?.nombres ?? "Sin paciente"} - {c.Dentistas?.nombre ?? "Sin dentista"} - {c.estado}",
                    start = c.fecha_hora.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = c.fecha_hora.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"), // Duración de 1 hora
                    estado = c.estado
                }).ToList();

                return Json(eventos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log del error
                System.Diagnostics.Debug.WriteLine($"Error en ObtenerEventos: {ex.Message}");
                return Json(new { error = "Error al obtener las citas" }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetClaseEventoCalendario(string estado)
        {
            switch (estado?.ToLower())
            {
                case "confirmada": return "fc-event-confirmada";
                case "pendiente": return "fc-event-pendiente";
                case "cancelada": return "fc-event-cancelada";
                case "completada": return "fc-event-completada";
                default: return "";
            }
        }

        private string GetClaseEvento(string estado)
        {
            switch (estado?.ToLower())
            {
                case "confirmada": return "evento-confirmado";
                case "pendiente": return "evento-pendiente";
                case "cancelada": return "evento-cancelado";
                case "completada": return "evento-completado";
                default: return "evento-default";
            }
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