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
    public class Tipo_CobroController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Tipo_Cobro
        public ActionResult Index()
        {
            return View(db.Tipo_Cobro.ToList());
        }

        // GET: Tipo_Cobro/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tipo_Cobro tipo_Cobro = db.Tipo_Cobro.Find(id);
            if (tipo_Cobro == null)
            {
                return HttpNotFound();
            }
            return View(tipo_Cobro);
        }

        // GET: Tipo_Cobro/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tipo_Cobro/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo_cobro,nombre")] Tipo_Cobro tipo_Cobro)
        {
            if (ModelState.IsValid)
            {
                db.Tipo_Cobro.Add(tipo_Cobro);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tipo_Cobro);
        }

        // GET: Tipo_Cobro/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tipo_Cobro tipo_Cobro = db.Tipo_Cobro.Find(id);
            if (tipo_Cobro == null)
            {
                return HttpNotFound();
            }
            return View(tipo_Cobro);
        }

        // POST: Tipo_Cobro/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tipo_cobro,nombre")] Tipo_Cobro tipo_Cobro)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tipo_Cobro).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tipo_Cobro);
        }

        // GET: Tipo_Cobro/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tipo_Cobro tipo_Cobro = db.Tipo_Cobro.Find(id);
            if (tipo_Cobro == null)
            {
                return HttpNotFound();
            }
            return View(tipo_Cobro);
        }

        // POST: Tipo_Cobro/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tipo_Cobro tipo_Cobro = db.Tipo_Cobro.Find(id);
            db.Tipo_Cobro.Remove(tipo_Cobro);
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
