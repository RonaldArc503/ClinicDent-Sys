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
    public class Historial_ClinicoController : Controller
    {
        private ClinicaDentalAzure db = new ClinicaDentalAzure();

        // GET: Historial_Clinico
        public ActionResult Index()
        {
            var historial_Clinico = db.Historial_Clinico.Include(h => h.Consultas_Tratamientos);
            return View(historial_Clinico.ToList());
        }

        // GET: Historial_Clinico/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Clinico historial_Clinico = db.Historial_Clinico.Find(id);
            if (historial_Clinico == null)
            {
                return HttpNotFound();
            }
            return View(historial_Clinico);
        }

        // GET: Historial_Clinico/Create
        public ActionResult Create()
        {
            ViewBag.id_consulta_tratamiento = new SelectList(db.Consultas_Tratamientos, "id_consulta_tratamiento", "id_consulta_tratamiento");
            return View();
        }

        // POST: Historial_Clinico/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_historial,id_consulta_tratamiento,fecha_registro,tipo")] Historial_Clinico historial_Clinico)
        {
            if (ModelState.IsValid)
            {
                db.Historial_Clinico.Add(historial_Clinico);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_consulta_tratamiento = new SelectList(db.Consultas_Tratamientos, "id_consulta_tratamiento", "id_consulta_tratamiento", historial_Clinico.id_consulta_tratamiento);
            return View(historial_Clinico);
        }

        // GET: Historial_Clinico/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Clinico historial_Clinico = db.Historial_Clinico.Find(id);
            if (historial_Clinico == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_consulta_tratamiento = new SelectList(db.Consultas_Tratamientos, "id_consulta_tratamiento", "id_consulta_tratamiento", historial_Clinico.id_consulta_tratamiento);
            return View(historial_Clinico);
        }

        // POST: Historial_Clinico/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_historial,id_consulta_tratamiento,fecha_registro,tipo")] Historial_Clinico historial_Clinico)
        {
            if (ModelState.IsValid)
            {
                db.Entry(historial_Clinico).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_consulta_tratamiento = new SelectList(db.Consultas_Tratamientos, "id_consulta_tratamiento", "id_consulta_tratamiento", historial_Clinico.id_consulta_tratamiento);
            return View(historial_Clinico);
        }

        // GET: Historial_Clinico/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Clinico historial_Clinico = db.Historial_Clinico.Find(id);
            if (historial_Clinico == null)
            {
                return HttpNotFound();
            }
            return View(historial_Clinico);
        }

        // POST: Historial_Clinico/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Historial_Clinico historial_Clinico = db.Historial_Clinico.Find(id);
            db.Historial_Clinico.Remove(historial_Clinico);
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
