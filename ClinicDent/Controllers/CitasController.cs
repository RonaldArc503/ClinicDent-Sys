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
        public ActionResult Index()
        {
            var citas = db.Citas.Include(c => c.Dentistas).Include(c => c.Pacientes);
            return View(citas.ToList());
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
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_cita,id_paciente,id_dentista,fecha_hora,estado")] Citas citas)
        {
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
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_cita,id_paciente,id_dentista,fecha_hora,estado")] Citas citas)
        {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Acción para mostrar el calendario
        public ActionResult Calendario()
        {
            return View();
        }


        // GET: Citas/Search
        public ActionResult Search(string searchTerm, string fechaHora, bool today = false, bool all = false)
        {
            var citas = db.Citas.Include(c => c.Dentistas).Include(c => c.Pacientes).AsQueryable();

            if (all)
            {
                // Solo mostramos todas las citas sin filtros
                return View("Index", citas.ToList());
            }
            // Search both patient and dentist names with single term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                citas = citas.Where(c =>
                    c.Pacientes.nombres.ToLower().Contains(searchTerm) ||
                    c.Dentistas.nombre.ToLower().Contains(searchTerm));
            }

            // Handle date filtering
            if (today)
            {
                citas = citas.Where(c => DbFunctions.TruncateTime(c.fecha_hora) == DbFunctions.TruncateTime(DateTime.Today));
            }

            else if (!string.IsNullOrEmpty(fechaHora))
            {
                if (DateTime.TryParse(fechaHora, out DateTime fecha))
                {
                    citas = citas.Where(c => DbFunctions.TruncateTime(c.fecha_hora) == DbFunctions.TruncateTime(fecha));
                }
            }

            return View("Index", citas.ToList());
        }

    }

}
