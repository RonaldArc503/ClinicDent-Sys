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
    public class PacientesController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Pacientes
        public ActionResult Index(string searchString = null, string filterBy = null, string estadoFilter = null)
        {
            var pacientes = db.Pacientes.AsQueryable();

            // Filtro por texto de búsqueda
            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                switch (filterBy)
                {
                    case "nombres":
                        pacientes = pacientes.Where(p => p.nombres.ToLower().Contains(searchString));
                        break;
                    case "apellidos":
                        pacientes = pacientes.Where(p => p.apellidos.ToLower().Contains(searchString));
                        break;
                    case "telefono":
                        pacientes = pacientes.Where(p => p.telefono.Contains(searchString));
                        break;
                    case "ambos":
                        pacientes = pacientes.Where(p =>
                            p.nombres.ToLower().Contains(searchString) ||
                            p.apellidos.ToLower().Contains(searchString));
                        break;
                    default:
                        pacientes = pacientes.Where(p =>
                            p.nombres.ToLower().Contains(searchString) ||
                            p.apellidos.ToLower().Contains(searchString) ||
                            p.telefono.Contains(searchString));
                        break;
                }
            }

            // Filtro por estado (activo/inactivo)
            if (!String.IsNullOrEmpty(estadoFilter) && estadoFilter != "Todos")
            {
                bool estado = estadoFilter == "Activos";
                pacientes = pacientes.Where(p => p.activo == estado);
            }

            // Pasar parámetros de búsqueda a la vista
            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.EstadoFilter = estadoFilter;

            return View(pacientes.OrderBy(p => p.apellidos).ThenBy(p => p.nombres).ToList());
        }

        // GET: Pacientes/Details/5
      


        // GET: Pacientes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Pacientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_paciente,nombres,apellidos,fecha_nacimiento,genero,telefono,alergias,activo")] Pacientes paciente)
        {
            if (ModelState.IsValid)
            {
                db.Pacientes.Add(paciente);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(paciente);
        }

        // GET: Pacientes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pacientes paciente = db.Pacientes.Find(id);
            if (paciente == null)
            {
                return HttpNotFound();
            }
            return View(paciente);
        }

        // POST: Pacientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_paciente,nombres,apellidos,fecha_nacimiento,genero,telefono,alergias,activo")] Pacientes paciente)
        {
            if (ModelState.IsValid)
            {
                db.Entry(paciente).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(paciente);
        }

        // GET: Pacientes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pacientes paciente = db.Pacientes.Find(id);
            if (paciente == null)
            {
                return HttpNotFound();
            }
            return View(paciente);
        }

        // POST: Pacientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pacientes paciente = db.Pacientes.Find(id);
            db.Pacientes.Remove(paciente);
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


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Cargar datos del paciente con relaciones
            var paciente = db.Pacientes
                .Include(p => p.Citas.Select(c => c.Dentistas))
                .Include(p => p.Consulta.Select(c => c.Dentistas))
                .Include(p => p.Consulta.Select(c => c.Citas))
                .FirstOrDefault(p => p.id_paciente == id);

            if (paciente == null)
            {
                return HttpNotFound();
            }

            // Obtener datos adicionales
            ViewBag.ProximasCitas = db.Citas
                .Include(c => c.Dentistas)
                .Where(c => c.id_paciente == id && c.fecha_hora >= DateTime.Now && c.estado != "Cancelada")
                .OrderBy(c => c.fecha_hora)
                .ToList();

            ViewBag.HistorialCitas = db.Citas
                .Include(c => c.Dentistas)
                .Where(c => c.id_paciente == id && c.fecha_hora < DateTime.Now)
                .OrderByDescending(c => c.fecha_hora)
                .Take(10)
                .ToList();

            ViewBag.ConsultasRecientes = db.Consulta
                .Include(c => c.Dentistas)
                .Where(c => c.id_paciente == id)
                .OrderByDescending(c => c.fecha_consulta)
                .Take(5)
                .ToList();

            ViewBag.Tratamientos = db.Consultas_Tratamientos
                .Include(ct => ct.Tratamientos.Tipo_Cobro)
                .Include(ct => ct.Consulta)
                .Where(ct => ct.Consulta.id_paciente == id)
                .OrderByDescending(ct => ct.Consulta.fecha_consulta)
                .ToList();

            ViewBag.Pagos = db.Pagos
                .Include(p => p.Pagos_Cuotas)
                .Where(p => p.Consulta.id_paciente == id)
                .OrderByDescending(p => p.fecha_pago)
                .ToList();

            return View(paciente);
        }
    }
}