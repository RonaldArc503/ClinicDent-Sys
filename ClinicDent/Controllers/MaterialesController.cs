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
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Materiales
        public ActionResult Index(string sortOrder, string filterBy, string searchString, string estadoFilter)
        {
            // Store current filter and sort parameters in ViewBag for the view
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.EstadoFilter = estadoFilter;

            // Get the materials query
            var materiales = from m in db.Materiales select m;

            // Apply text search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                switch (filterBy)
                {
                    case "proveedor":
                        materiales = materiales.Where(m => m.proveedor != null && m.proveedor.ToLower().Contains(searchString));
                        break;
                    case "descripcion":
                        materiales = materiales.Where(m => m.descripcion != null && m.descripcion.ToLower().Contains(searchString));
                        break;
                    case "nombre":
                    default:
                        materiales = materiales.Where(m => m.nombre != null && m.nombre.ToLower().Contains(searchString));
                        break;
                }
            }

            // Apply estado filter
            if (!string.IsNullOrEmpty(estadoFilter))
            {
                materiales = materiales.Where(m => m.estado == estadoFilter);
            }

            // Apply sorting
            switch (sortOrder)
            {
                case "nombre_desc":
                    materiales = materiales.OrderByDescending(m => m.nombre);
                    break;
                case "cantidad_asc":
                    materiales = materiales.OrderBy(m => m.cantidad);
                    break;
                case "cantidad_desc":
                    materiales = materiales.OrderByDescending(m => m.cantidad);
                    break;
                case "caducidad_asc":
                    materiales = materiales.OrderBy(m => m.fecha_caducidad);
                    break;
                case "caducidad_desc":
                    materiales = materiales.OrderByDescending(m => m.fecha_caducidad);
                    break;
                case "nombre_asc":
                default:
                    materiales = materiales.OrderBy(m => m.nombre);
                    break;
            }

            return View(materiales.ToList());
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_material,nombre,cantidad,proveedor,fecha_caducidad,minimo_stock,descripcion,estado")] Materiales materiales)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    if (ModelState.IsValid)
                    {
                        db.Materiales.Add(materiales);
                        db.SaveChanges();
                        return Json(new { success = true, message = "Material creado correctamente." });
                    }

                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Errores de validación: " + string.Join("; ", errors) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Error al crear el material: " + ex.Message });
                }
            }

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