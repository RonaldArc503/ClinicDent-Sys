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
    public class ConsultasController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Consultas
        public ActionResult Index(string searchString, string filterBy, string fechaFilter)
        {
            var consultas = db.Consulta
                .Include(c => c.Citas)
                .Include(c => c.Dentistas)
                .Include(c => c.Pacientes)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                switch (filterBy)
                {
                    case "paciente":
                        consultas = consultas.Where(c => c.Pacientes.nombres.ToLower().Contains(searchString));
                        break;
                    case "dentista":
                        consultas = consultas.Where(c => c.Dentistas.nombre.ToLower().Contains(searchString));
                        break;
                    case "diagnostico":
                        consultas = consultas.Where(c => c.diagnostico != null && c.diagnostico.ToLower().Contains(searchString));
                        break;
                    default:
                        consultas = consultas.Where(c =>
                            c.Pacientes.nombres.ToLower().Contains(searchString) ||
                            c.Dentistas.nombre.ToLower().Contains(searchString) ||
                            (c.diagnostico != null && c.diagnostico.ToLower().Contains(searchString)));
                        break;
                }
            }

            if (!String.IsNullOrEmpty(fechaFilter))
            {
                switch (fechaFilter)
                {
                    case "Hoy":
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) == DbFunctions.TruncateTime(DateTime.Today));
                        break;
                    case "Semana":
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(7);
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) >= startOfWeek && DbFunctions.TruncateTime(c.fecha_consulta) < endOfWeek);
                        break;
                    case "Mes":
                        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1);
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) >= startOfMonth && DbFunctions.TruncateTime(c.fecha_consulta) < endOfMonth);
                        break;
                }
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.FechaFilter = fechaFilter;

            return View(consultas.OrderBy(c => c.fecha_consulta).ToList());
        }

        // GET: Consultas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }
            return View(consulta);
        }

        // GET: Consultas/Create
        public ActionResult Create(int? idCita, string fechaConsulta, int? idDentista, int? idPaciente)
        {
            var citasPendientesCanceladas = db.Citas
                .Where(c => c.estado == "Pendiente" || c.estado == "Cancelada")
                .Include(c => c.Pacientes)
                .ToList()
                .Select(c => new
                {
                    c.id_cita,
                    Display = $"{c.id_cita} - {c.Pacientes.nombres} - {c.fecha_hora.ToString("dd MMM yyyy HH:mm")}"
                });

            ViewBag.id_cita = new SelectList(citasPendientesCanceladas, "id_cita", "Display", idCita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", idDentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", idPaciente);

            var model = new Consulta();

            if (idCita != null)
            {
                var cita = db.Citas.Find(idCita);
                if (cita != null)
                {
                    model.id_cita = cita.id_cita;
                    model.fecha_consulta = cita.fecha_hora;
                    ViewBag.FechaConsulta = cita.fecha_hora.ToString("dd MMM. yyyy HH:mm", new System.Globalization.CultureInfo("es-ES"));
                    if (idDentista != null)
                    {
                        model.id_dentista = idDentista.Value;
                    }
                    if (idPaciente != null)
                    {
                        model.id_paciente = idPaciente.Value;
                    }
                }
            }
            else
            {
                model.fecha_consulta = DateTime.Now;
            }

            return View(model);
        }

        // POST: Consultas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_consulta,id_cita,id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            // If id_cita is not selected or invalid, set it to null (sin cita)
            if (consulta.id_cita == null || consulta.id_cita == 0)
            {
                consulta.id_cita = null;
            }
            else
            {
                // Validate that the selected cita is not already associated with another consulta
                if (db.Consulta.Any(c => c.id_cita == consulta.id_cita))
                {
                    ModelState.AddModelError("id_cita", "Esta cita ya tiene una consulta asociada.");
                }
            }

            // Validate fecha_consulta to ensure it is not default
            if (consulta.fecha_consulta == default(DateTime))
            {
                ModelState.AddModelError("fecha_consulta", "Por favor, especifique una fecha y hora válidas para la consulta.");
            }

            if (ModelState.IsValid)
            {
                db.Consulta.Add(consulta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reinitialize ViewBag for dropdowns
            var citasPendientesCanceladas = db.Citas
                .Where(c => c.estado == "Pendiente" || c.estado == "Cancelada")
                .Include(c => c.Pacientes)
                .ToList()
                .Select(c => new
                {
                    c.id_cita,
                    Display = $"{c.id_cita} - {c.Pacientes.nombres} - {c.fecha_hora.ToString("dd MMM yyyy HH:mm")}"
                });

            ViewBag.id_cita = new SelectList(citasPendientesCanceladas, "id_cita", "Display", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);

            return View(consulta);
        }

        // GET: Consultas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }

            var citasPendientesCanceladas = db.Citas
                .Where(c => c.estado == "Pendiente" || c.estado == "Cancelada")
                .Include(c => c.Pacientes)
                .ToList()
                .Select(c => new
                {
                    c.id_cita,
                    Display = $"{c.id_cita} - {c.Pacientes.nombres} - {c.fecha_hora.ToString("dd MMM yyyy HH:mm")}"
                });

            ViewBag.id_cita = new SelectList(citasPendientesCanceladas, "id_cita", "Display", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);
            return View(consulta);
        }

        // POST: Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_consulta,id_cita,id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            // If id_cita is not selected or invalid, set it to null (sin cita)
            if (consulta.id_cita == null || consulta.id_cita == 0)
            {
                consulta.id_cita = null;
            }
            else
            {
                // Validate that the selected cita is not already associated with another consulta (except this one)
                if (db.Consulta.Any(c => c.id_cita == consulta.id_cita && c.id_consulta != consulta.id_consulta))
                {
                    ModelState.AddModelError("id_cita", "Esta cita ya tiene una consulta asociada.");
                }
            }

            // Validate fecha_consulta to ensure it is not default
            if (consulta.fecha_consulta == default(DateTime))
            {
                ModelState.AddModelError("fecha_consulta", "Por favor, especifique una fecha y hora válidas para la consulta.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(consulta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reinitialize ViewBag for dropdowns
            var citasPendientesCanceladas = db.Citas
                .Where(c => c.estado == "Pendiente" || c.estado == "Cancelada")
                .Include(c => c.Pacientes)
                .ToList()
                .Select(c => new
                {
                    c.id_cita,
                    Display = $"{c.id_cita} - {c.Pacientes.nombres} - {c.fecha_hora.ToString("dd MMM yyyy HH:mm")}"
                });

            ViewBag.id_cita = new SelectList(citasPendientesCanceladas, "id_cita", "Display", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);
            return View(consulta);
        }

        // GET: Consultas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }
            return View(consulta);
        }

        // POST: Consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Consulta consulta = db.Consulta.Find(id);
            db.Consulta.Remove(consulta);
            db.SaveChanges();
            return RedirectToAction("Index");
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