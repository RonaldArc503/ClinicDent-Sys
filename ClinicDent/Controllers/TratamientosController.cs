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
    public class TratamientosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Tratamientos
        public ActionResult Index()
        {
            var tratamientos = db.Tratamientos.Include(t => t.Tipo_Cobro);
            return View(tratamientos.ToList());
        }

        // GET: Tratamientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamientos = db.Tratamientos.Find(id);
            if (tratamientos == null)
            {
                return HttpNotFound();
            }
            return View(tratamientos);
        }

        // GET: Tratamientos/Create
        public ActionResult Create()
        {
            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre");
            return View();
        }

        // POST: Tratamientos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tratamiento,id_tipo_cobro,fecha_inicio,odontograma,costo,duracion_estimada,seguimiento")] Tratamientos tratamientos)
        {
            if (ModelState.IsValid)
            {
                db.Tratamientos.Add(tratamientos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre", tratamientos.id_tipo_cobro);
            return View(tratamientos);
        }

        // GET: Tratamientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamientos = db.Tratamientos.Find(id);
            if (tratamientos == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre", tratamientos.id_tipo_cobro);
            return View(tratamientos);
        }

        // POST: Tratamientos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tratamiento,id_tipo_cobro,fecha_inicio,odontograma,costo,duracion_estimada,seguimiento")] Tratamientos tratamientos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tratamientos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre", tratamientos.id_tipo_cobro);
            return View(tratamientos);
        }

        // GET: Tratamientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamientos = db.Tratamientos.Find(id);
            if (tratamientos == null)
            {
                return HttpNotFound();
            }
            return View(tratamientos);
        }

        // POST: Tratamientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tratamientos tratamientos = db.Tratamientos.Find(id);
            db.Tratamientos.Remove(tratamientos);
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
