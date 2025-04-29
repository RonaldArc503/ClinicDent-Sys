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
            var tratamientos = db.Tratamientos.Include(t => t.Tipo_Cobro)
                                .OrderByDescending(t => t.fecha_inicio)
                                .ToList();
            return View(tratamientos);
        }

        // GET: Tratamientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamiento = db.Tratamientos.Find(id);
            if (tratamiento == null)
            {
                return HttpNotFound();
            }
            return View(tratamiento);
        }

        // GET: Tratamientos/Create
        public ActionResult Create()
        {
            // Cargar tipos de cobro ordenados alfabéticamente
            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre");

            // Establecer fecha por defecto como hoy
            ViewBag.FechaActual = DateTime.Now.ToString("dd/MM/yyyy");

            return View();
        }

        // POST: Tratamientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tratamiento,id_tipo_cobro,fecha_inicio,odontograma,costo,duracion_estimada,seguimiento")] Tratamientos tratamiento)
        {
            if (ModelState.IsValid)
            {
                // Validar costo mínimo
                if (tratamiento.costo <= 0)
                {
                    ModelState.AddModelError("costo", "El costo debe ser mayor a cero");
                }
                else
                {
                    // Validar fecha no futura (opcional, descomentar si se necesita)
                    // if (tratamiento.fecha_inicio > DateTime.Today)
                    // {
                    //     ModelState.AddModelError("fecha_inicio", "La fecha no puede ser futura");
                    // }
                    // else
                    // {
                    db.Tratamientos.Add(tratamiento);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                    // }
                }
            }

            // Si hay errores, recargar la lista de tipos de cobro
            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre", tratamiento.id_tipo_cobro);
            return View(tratamiento);
        }

        // GET: Tratamientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamiento = db.Tratamientos.Find(id);
            if (tratamiento == null)
            {
                return HttpNotFound();
            }

            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre",
                tratamiento.id_tipo_cobro);

            return View(tratamiento);
        }

        // POST: Tratamientos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tratamiento,id_tipo_cobro,fecha_inicio,odontograma,costo,duracion_estimada,seguimiento")] Tratamientos tratamiento)
        {
            if (ModelState.IsValid)
            {
                // Validación de costo mínimo
                if (tratamiento.costo <= 0)
                {
                    ModelState.AddModelError("costo", "El costo debe ser mayor a cero");
                }
                else
                {
                    db.Entry(tratamiento).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }

            ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre", tratamiento.id_tipo_cobro);
            return View(tratamiento);
        }

        // GET: Tratamientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamiento = db.Tratamientos.Find(id);
            if (tratamiento == null)
            {
                return HttpNotFound();
            }
            return View(tratamiento);
        }

        // POST: Tratamientos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tratamientos tratamiento = db.Tratamientos.Find(id);
            db.Tratamientos.Remove(tratamiento);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Método para obtener los tipos de cobro (puede usarse para AJAX)
        public JsonResult GetTiposCobro()
        {
            var tipos = db.Tipo_Cobro
                         .OrderBy(t => t.nombre)
                         .Select(t => new {
                             id = t.id_tipo_cobro,
                             nombre = t.nombre
                         })
                         .ToList();
            return Json(tipos, JsonRequestBehavior.AllowGet);
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