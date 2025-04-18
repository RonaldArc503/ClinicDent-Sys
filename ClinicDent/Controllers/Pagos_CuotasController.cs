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
    public class Pagos_CuotasController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Pagos_Cuotas
        public ActionResult Index()
        {
            var pagos_Cuotas = db.Pagos_Cuotas.Include(p => p.Pagos);
            return View(pagos_Cuotas.ToList());
        }

        // GET: Pagos_Cuotas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagos_Cuotas pagos_Cuotas = db.Pagos_Cuotas.Find(id);
            if (pagos_Cuotas == null)
            {
                return HttpNotFound();
            }
            return View(pagos_Cuotas);
        }

        // GET: Pagos_Cuotas/Create
        public ActionResult Create()
        {
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago");
            return View();
        }

        // POST: Pagos_Cuotas/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_cuota,id_pago,fecha_pago,monto,estado")] Pagos_Cuotas pagos_Cuotas)
        {
            if (ModelState.IsValid)
            {
                db.Pagos_Cuotas.Add(pagos_Cuotas);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", pagos_Cuotas.id_pago);
            return View(pagos_Cuotas);
        }

        // GET: Pagos_Cuotas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagos_Cuotas pagos_Cuotas = db.Pagos_Cuotas.Find(id);
            if (pagos_Cuotas == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", pagos_Cuotas.id_pago);
            return View(pagos_Cuotas);
        }

        // POST: Pagos_Cuotas/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_cuota,id_pago,fecha_pago,monto,estado")] Pagos_Cuotas pagos_Cuotas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pagos_Cuotas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", pagos_Cuotas.id_pago);
            return View(pagos_Cuotas);
        }

        // GET: Pagos_Cuotas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Pagos_Cuotas pagos_Cuotas = db.Pagos_Cuotas.Find(id);
            if (pagos_Cuotas == null)
            {
                return HttpNotFound();
            }
            return View(pagos_Cuotas);
        }

        // POST: Pagos_Cuotas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pagos_Cuotas pagos_Cuotas = db.Pagos_Cuotas.Find(id);
            db.Pagos_Cuotas.Remove(pagos_Cuotas);
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
