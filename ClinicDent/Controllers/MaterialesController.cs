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
    public class MaterialesController : Controller
    {
        private ClinicaDentalAzure db = new ClinicaDentalAzure();

        // GET: Materiales
        public ActionResult Index()
        {
            return View(db.Materiales.ToList());
        }

        // GET: Materiales/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Materiales materiales = db.Materiales.Find(id);
            if (materiales == null)
            {
                return HttpNotFound();
            }
            return View(materiales);
        }

        // GET: Materiales/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Materiales/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_material,nombre,cantidad,proveedor,fecha_caducidad,minimo_stock,descripcion,estado")] Materiales materiales)
        {
            if (ModelState.IsValid)
            {
                db.Materiales.Add(materiales);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(materiales);
        }

        // GET: Materiales/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Materiales materiales = db.Materiales.Find(id);
            if (materiales == null)
            {
                return HttpNotFound();
            }
            return View(materiales);
        }

        // POST: Materiales/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_material,nombre,cantidad,proveedor,fecha_caducidad,minimo_stock,descripcion,estado")] Materiales materiales)
        {
            if (ModelState.IsValid)
            {
                db.Entry(materiales).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(materiales);
        }

        // GET: Materiales/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Materiales materiales = db.Materiales.Find(id);
            if (materiales == null)
            {
                return HttpNotFound();
            }
            return View(materiales);
        }

        // POST: Materiales/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Materiales materiales = db.Materiales.Find(id);
            db.Materiales.Remove(materiales);
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
