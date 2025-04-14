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
        private ClinicaDentalLocal db = new ClinicaDentalLocal();

        // GET: Dentistas
        public ActionResult Index(string searchString, string filterBy)
        {
            var dentistas = db.Dentistas.AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                switch (filterBy)
                {
                    case "nombre":
                        dentistas = dentistas.Where(d => d.nombre.ToLower().Contains(searchString));
                        break;
                    case "apellido":
                        dentistas = dentistas.Where(d => d.apellido.ToLower().Contains(searchString));
                        break;
                    case "especialidad":
                        dentistas = dentistas.Where(d => d.especialidad.ToLower().Contains(searchString));
                        break;
                    case "correo":
                        dentistas = dentistas.Where(d => d.correo.ToLower().Contains(searchString));
                        break;
                    case "telefono":
                        dentistas = dentistas.Where(d => d.telefono.Contains(searchString));
                        break;
                    default:
                        // Búsqueda general en todos los campos
                        dentistas = dentistas.Where(d =>
                            d.nombre.ToLower().Contains(searchString) ||
                            d.apellido.ToLower().Contains(searchString) ||
                            d.especialidad.ToLower().Contains(searchString) ||
                            d.correo.ToLower().Contains(searchString) ||
                            d.telefono.Contains(searchString));
                        break;
                }
            }

            // Pasar los parámetros de búsqueda a la vista para mantener el filtro
            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;

            return View(dentistas.ToList());
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