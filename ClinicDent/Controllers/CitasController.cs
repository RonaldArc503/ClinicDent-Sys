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
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Create([Bind(Include = "id_cita,id_paciente,id_dentista,estado")] Citas citas, string fecha_hora)
        {
            try
            {
                // Parse fecha_hora
                if (!DateTime.TryParse(fecha_hora, out DateTime parsedFechaHora))
                {
                    return Json(new { success = false, message = "Formato de fecha_hora inválido." });
                }
                citas.fecha_hora = parsedFechaHora;

                // Validar que no exista cita con el mismo dentista en el mismo horario exacto
                bool citaExactaMismoDentista = db.Citas.Any(c =>
                    c.id_dentista == citas.id_dentista &&
                    c.fecha_hora == citas.fecha_hora &&
                    c.estado != "Cancelada");

                if (citaExactaMismoDentista)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Ya existe una cita con este dentista a la misma fecha y hora exacta.",
                        motivo = "dentista_ocupado_exacto"
                    });
                }

                // Validar intervalo de 40 minutos para el mismo dentista
                DateTime horaInicio40 = citas.fecha_hora.AddMinutes(-40);
                DateTime horaFin40 = citas.fecha_hora.AddMinutes(40);

                bool citaEnIntervaloDentista = db.Citas.Any(c =>
                    c.id_dentista == citas.id_dentista &&
                    c.fecha_hora >= horaInicio40 &&
                    c.fecha_hora <= horaFin40 &&
                    c.estado != "Cancelada");

                if (citaEnIntervaloDentista)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El dentista ya tiene una cita programada en un intervalo de 40 minutos.",
                        motivo = "dentista_ocupado_intervalo"
                    });
                }

                // Validar intervalo de 1 hora para el mismo paciente y dentista
                DateTime horaInicio = citas.fecha_hora.AddHours(-1);
                DateTime horaFin = citas.fecha_hora.AddHours(1);

                bool citaEnIntervaloPaciente = db.Citas.Any(c =>
                    c.id_dentista == citas.id_dentista &&
                    c.id_paciente == citas.id_paciente &&
                    c.fecha_hora >= horaInicio &&
                    c.fecha_hora <= horaFin &&
                    c.id_cita != citas.id_cita &&
                    c.estado != "Cancelada");

                if (citaEnIntervaloPaciente)
                {
                    return Json(new
                    {
                        success = false,
                        message = "El paciente ya tiene una cita con este dentista en un intervalo de 1 hora. Por favor, seleccione otro horario.",
                        motivo = "paciente_con_dentista"
                    });
                }

                if (ModelState.IsValid)
                {
                    db.Citas.Add(citas);
                    db.SaveChanges();
                    return Json(new { success = true, message = "La cita se ha programado correctamente." });
                }

                // Collect validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Errores de validación: " + string.Join("; ", errors) });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Create: {ex.Message}\n{ex.StackTrace}");
                return Json(new { success = false, message = "Error al crear la cita: " + ex.Message });
            }
        }

        // GET: Citas/ValidarDisponibilidad
        [HttpGet]
        public JsonResult ValidarDisponibilidad(int id_dentista, int id_paciente, string fecha_hora, string hora_inicio, string hora_fin)
        {
            try
            {
                // Parse ISO date strings
                if (!DateTime.TryParse(fecha_hora, out DateTime fechaHora))
                {
                    return Json(new
                    {
                        disponible = false,
                        mensaje = "Formato de fecha_hora inválido."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!DateTime.TryParse(hora_inicio, out DateTime horaInicio))
                {
                    return Json(new
                    {
                        disponible = false,
                        mensaje = "Formato de hora_inicio inválido."
                    }, JsonRequestBehavior.AllowGet);
                }

                if (!DateTime.TryParse(hora_fin, out DateTime horaFin))
                {
                    return Json(new
                    {
                        disponible = false,
                        mensaje = "Formato de hora_fin inválido."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Log parameters for debugging
                System.Diagnostics.Debug.WriteLine($"ValidarDisponibilidad: id_dentista={id_dentista}, id_paciente={id_paciente}, fecha_hora={fecha_hora}, hora_inicio={hora_inicio}, hora_fin={hora_fin}");

                // Validate exact same dentist appointment
                var citaExistenteMismoDentista = db.Citas.Any(c =>
                    c.id_dentista == id_dentista &&
                    c.fecha_hora == fechaHora &&
                    c.estado != "Cancelada");

                if (citaExistenteMismoDentista)
                {
                    return Json(new
                    {
                        disponible = false,
                        motivo = "dentista_ocupado",
                        mensaje = "El dentista ya tiene una cita programada para esta fecha y hora exacta."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Validate 1-hour interval for same patient and dentist
                var citaExistentePacienteDentista = db.Citas.Any(c =>
                    c.id_dentista == id_dentista &&
                    c.id_paciente == id_paciente &&
                    c.fecha_hora >= horaInicio &&
                    c.fecha_hora <= horaFin &&
                    c.estado != "Cancelada");

                if (citaExistentePacienteDentista)
                {
                    return Json(new
                    {
                        disponible = false,
                        motivo = "paciente_con_dentista",
                        mensaje = "El paciente ya tiene una cita con este dentista en un intervalo de 1 hora."
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { disponible = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ValidarDisponibilidad: {ex.Message}\n{ex.StackTrace}");
                return Json(new
                {
                    disponible = false,
                    mensaje = "Error al validar disponibilidad: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
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

        // GET: Citas/Calendario
        public ActionResult Calendario()
        {
            return View();
        }

        // GET: Citas/ObtenerEventos
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
                    end = c.fecha_hora.AddHours(1).ToString("yyyy-MM-ddTHH:mm:ss"),
                    estado = c.estado
                }).ToList();

                return Json(eventos, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
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

        // GET: Citas/GenerarConsulta/5
        public ActionResult GenerarConsulta(int idCita)
        {
            var cita = db.Citas
                .Include(c => c.Pacientes)
                .Include(c => c.Dentistas)
                .FirstOrDefault(c => c.id_cita == idCita);

            if (cita == null)
            {
                return HttpNotFound();
            }

            // Verificar si ya existe una consulta para esta cita
            if (db.Consulta.Any(c => c.id_cita == idCita))
            {
                TempData["WarningMessage"] = "Esta cita ya tiene una consulta asociada.";
                return RedirectToAction("Index");
            }

            // Formatear la fecha como "16 abr. 2025 15:02"
            string fechaFormateada = cita.fecha_hora.ToString("dd MMM. yyyy HH:mm", new System.Globalization.CultureInfo("es-ES"));

            // Redirigir al formulario de creación de consultas con los datos precargados
            return RedirectToAction("Create", "Consultas", new
            {
                idCita = idCita,
                fechaConsulta = fechaFormateada,
                idDentista = cita.id_dentista,
                idPaciente = cita.id_paciente,
                nombreDentista = cita.Dentistas?.nombre,
                nombrePaciente = cita.Pacientes?.nombres
            });
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