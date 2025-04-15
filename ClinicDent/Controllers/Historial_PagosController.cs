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
    public class Historial_PagosController : Controller
    {
        private ClinicaDentalLocal db = new ClinicaDentalLocal();

        // GET: Historial_Pagos
        public ActionResult Index()
        {
            var historial_Pagos = db.Historial_Pagos.Include(h => h.Pagos_Cuotas).Include(h => h.Pagos);
            return View(historial_Pagos.ToList());
        }

        // GET: Historial_Pagos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Pagos historial_Pagos = db.Historial_Pagos.Find(id);
            if (historial_Pagos == null)
            {
                return HttpNotFound();
            }
            return View(historial_Pagos);
        }

        // GET: Historial_Pagos/Create
        public ActionResult Create()
        {
            ViewBag.id_cuota = new SelectList(db.Pagos_Cuotas, "id_cuota", "estado");
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago");
            return View();
        }

        // POST: Historial_Pagos/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_historial_pago,id_cuota,id_pago,estado")] Historial_Pagos historial_Pagos)
        {
            if (ModelState.IsValid)
            {
                db.Historial_Pagos.Add(historial_Pagos);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_cuota = new SelectList(db.Pagos_Cuotas, "id_cuota", "estado", historial_Pagos.id_cuota);
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", historial_Pagos.id_pago);
            return View(historial_Pagos);
        }

        // GET: Historial_Pagos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Pagos historial_Pagos = db.Historial_Pagos.Find(id);
            if (historial_Pagos == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_cuota = new SelectList(db.Pagos_Cuotas, "id_cuota", "estado", historial_Pagos.id_cuota);
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", historial_Pagos.id_pago);
            return View(historial_Pagos);
        }

        // POST: Historial_Pagos/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_historial_pago,id_cuota,id_pago,estado")] Historial_Pagos historial_Pagos)
        {
            if (ModelState.IsValid)
            {
                db.Entry(historial_Pagos).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_cuota = new SelectList(db.Pagos_Cuotas, "id_cuota", "estado", historial_Pagos.id_cuota);
            ViewBag.id_pago = new SelectList(db.Pagos, "id_pago", "metodo_pago", historial_Pagos.id_pago);
            return View(historial_Pagos);
        }

        // GET: Historial_Pagos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Historial_Pagos historial_Pagos = db.Historial_Pagos.Find(id);
            if (historial_Pagos == null)
            {
                return HttpNotFound();
            }
            return View(historial_Pagos);
        }

        // POST: Historial_Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Historial_Pagos historial_Pagos = db.Historial_Pagos.Find(id);
            db.Historial_Pagos.Remove(historial_Pagos);
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
