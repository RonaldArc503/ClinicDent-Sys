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
    public class Consultas_TratamientosController : Controller
    {
        private ClinicaDentalAzure db = new ClinicaDentalAzure();

        // GET: Consultas_Tratamientos
        public ActionResult Index()
        {
            var consultas_Tratamientos = db.Consultas_Tratamientos.Include(c => c.Consulta).Include(c => c.Tratamientos);
            return View(consultas_Tratamientos.ToList());
        }

        // GET: Consultas_Tratamientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consultas_Tratamientos consultas_Tratamientos = db.Consultas_Tratamientos.Find(id);
            if (consultas_Tratamientos == null)
            {
                return HttpNotFound();
            }
            return View(consultas_Tratamientos);
        }

        // GET: Consultas_Tratamientos/Create
        public ActionResult Create()
        {
            ViewBag.id_consulta = new SelectList(db.Consulta, "id_consulta", "diagnostico");
            ViewBag.id_tratamiento = new SelectList(db.Tratamientos, "id_tratamiento", "odontograma");
            return View();
        }

        // POST: Consultas_Tratamientos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_consulta_tratamiento,id_consulta,id_tratamiento,cantidad,total")] Consultas_Tratamientos consultas_Tratamientos)
        {
            if (ModelState.IsValid)
            {
                db.Consultas_Tratamientos.Add(consultas_Tratamientos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_consulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", consultas_Tratamientos.id_consulta);
            ViewBag.id_tratamiento = new SelectList(db.Tratamientos, "id_tratamiento", "odontograma", consultas_Tratamientos.id_tratamiento);
            return View(consultas_Tratamientos);
        }

        // GET: Consultas_Tratamientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consultas_Tratamientos consultas_Tratamientos = db.Consultas_Tratamientos.Find(id);
            if (consultas_Tratamientos == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_consulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", consultas_Tratamientos.id_consulta);
            ViewBag.id_tratamiento = new SelectList(db.Tratamientos, "id_tratamiento", "odontograma", consultas_Tratamientos.id_tratamiento);
            return View(consultas_Tratamientos);
        }

        // POST: Consultas_Tratamientos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_consulta_tratamiento,id_consulta,id_tratamiento,cantidad,total")] Consultas_Tratamientos consultas_Tratamientos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consultas_Tratamientos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_consulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", consultas_Tratamientos.id_consulta);
            ViewBag.id_tratamiento = new SelectList(db.Tratamientos, "id_tratamiento", "odontograma", consultas_Tratamientos.id_tratamiento);
            return View(consultas_Tratamientos);
        }

        // GET: Consultas_Tratamientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consultas_Tratamientos consultas_Tratamientos = db.Consultas_Tratamientos.Find(id);
            if (consultas_Tratamientos == null)
            {
                return HttpNotFound();
            }
            return View(consultas_Tratamientos);
        }

        // POST: Consultas_Tratamientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Consultas_Tratamientos consultas_Tratamientos = db.Consultas_Tratamientos.Find(id);
            db.Consultas_Tratamientos.Remove(consultas_Tratamientos);
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
