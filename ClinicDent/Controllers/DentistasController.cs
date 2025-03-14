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
    public class DentistasController : Controller
    {
        private ClinicDentDBEntities db = new ClinicDentDBEntities();

        // GET: Dentistas
        public ActionResult Index()
        {
            return View(db.Dentistas.ToList());
        }

        // GET: Dentistas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dentistas dentistas = db.Dentistas.Find(id);
            if (dentistas == null)
            {
                return HttpNotFound();
            }
            return View(dentistas);
        }

        // GET: Dentistas/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Dentistas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_dentista,nombre,apellido,telefono,correo,especialidad,activo")] Dentistas dentistas)
        {
            if (ModelState.IsValid)
            {
                db.Dentistas.Add(dentistas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(dentistas);
        }

        // GET: Dentistas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dentistas dentistas = db.Dentistas.Find(id);
            if (dentistas == null)
            {
                return HttpNotFound();
            }
            return View(dentistas);
        }

        // POST: Dentistas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_dentista,nombre,apellido,telefono,correo,especialidad,activo")] Dentistas dentistas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(dentistas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(dentistas);
        }

        // GET: Dentistas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Dentistas dentistas = db.Dentistas.Find(id);
            if (dentistas == null)
            {
                return HttpNotFound();
            }
            return View(dentistas);
        }

        // POST: Dentistas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Dentistas dentistas = db.Dentistas.Find(id);
            db.Dentistas.Remove(dentistas);
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
