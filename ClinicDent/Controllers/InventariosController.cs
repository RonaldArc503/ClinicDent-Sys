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
    public class InventariosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Inventarios
        public ActionResult Index(string sortOrder, string filterBy, string searchString,
                                string cantidadFilter, string fechaFilter, string proveedorFilter,
                                int? minCantidad, int? maxCantidad)
        {
            // Configurar ViewBag para mantener el estado de los filtros
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.CantidadFilter = cantidadFilter;
            ViewBag.FechaFilter = fechaFilter;
            ViewBag.ProveedorFilter = proveedorFilter;
            ViewBag.MinCantidad = minCantidad;
            ViewBag.MaxCantidad = maxCantidad;

            // Obtener todos los registros de inventario con la relación de Materiales
            var inventario = db.Inventario.Include(i => i.Materiales);

            #region Aplicar Filtros

            // Filtro por texto (nombre o descripción)
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                switch (filterBy)
                {
                    case "nombre":
                        inventario = inventario.Where(i => i.Materiales.nombre != null &&
                                                         i.Materiales.nombre.ToLower().Contains(searchString));
                        break;
                    case "descripcion":
                        inventario = inventario.Where(i => i.Materiales.descripcion != null &&
                                                         i.Materiales.descripcion.ToLower().Contains(searchString));
                        break;
                }
            }

            // Filtro por estado de cantidad
            if (!string.IsNullOrEmpty(cantidadFilter))
            {
                switch (cantidadFilter)
                {
                    case "bajo_stock":
                        inventario = inventario.Where(i => i.cantidad < (i.Materiales.minimo_stock ?? 0));
                        break;
                    case "stock_ok":
                        inventario = inventario.Where(i => i.cantidad >= (i.Materiales.minimo_stock ?? 0));
                        break;
                    case "agotado":
                        inventario = inventario.Where(i => i.cantidad == 0);
                        break;
                }
            }

            // Filtro por rango de cantidades
            if (minCantidad.HasValue)
            {
                inventario = inventario.Where(i => i.cantidad >= minCantidad.Value);
            }
            if (maxCantidad.HasValue)
            {
                inventario = inventario.Where(i => i.cantidad <= maxCantidad.Value);
            }

            // Filtro por fecha de actualización
            if (!string.IsNullOrEmpty(fechaFilter))
            {
                DateTime fechaLimite;
                switch (fechaFilter)
                {
                    case "hoy":
                        fechaLimite = DateTime.Today;
                        inventario = inventario.Where(i => DbFunctions.TruncateTime(i.fecha_actualizacion) == fechaLimite);
                        break;
                    case "semana":
                        fechaLimite = DateTime.Today.AddDays(-7);
                        inventario = inventario.Where(i => DbFunctions.TruncateTime(i.fecha_actualizacion) >= fechaLimite);
                        break;
                    case "mes":
                        fechaLimite = DateTime.Today.AddMonths(-1);
                        inventario = inventario.Where(i => DbFunctions.TruncateTime(i.fecha_actualizacion) >= fechaLimite);
                        break;
                    case "anio":
                        fechaLimite = DateTime.Today.AddYears(-1);
                        inventario = inventario.Where(i => DbFunctions.TruncateTime(i.fecha_actualizacion) >= fechaLimite);
                        break;
                }
            }

            // Filtro por proveedor
            if (!string.IsNullOrEmpty(proveedorFilter))
            {
                inventario = inventario.Where(i => i.Materiales.proveedor != null &&
                                                i.Materiales.proveedor.ToLower() == proveedorFilter.ToLower());
            }

            #endregion

            #region Aplicar Ordenamiento

            // Configurar el ordenamiento
            ViewBag.NombreSortParam = string.IsNullOrEmpty(sortOrder) ? "nombre_desc" : "";
            ViewBag.CantidadSortParam = sortOrder == "cantidad_asc" ? "cantidad_desc" : "cantidad_asc";
            ViewBag.FechaSortParam = sortOrder == "fecha_asc" ? "fecha_desc" : "fecha_asc";
            ViewBag.StockSortParam = sortOrder == "stock_asc" ? "stock_desc" : "stock_asc";
            ViewBag.ProveedorSortParam = sortOrder == "proveedor_asc" ? "proveedor_desc" : "proveedor_asc";

            switch (sortOrder)
            {
                case "nombre_desc":
                    inventario = inventario.OrderByDescending(i => i.Materiales.nombre);
                    break;
                case "cantidad_asc":
                    inventario = inventario.OrderBy(i => i.cantidad);
                    break;
                case "cantidad_desc":
                    inventario = inventario.OrderByDescending(i => i.cantidad);
                    break;
                case "fecha_asc":
                    inventario = inventario.OrderBy(i => i.fecha_actualizacion);
                    break;
                case "fecha_desc":
                    inventario = inventario.OrderByDescending(i => i.fecha_actualizacion);
                    break;
                case "stock_asc":
                    inventario = inventario.OrderBy(i => i.cantidad - (i.Materiales.minimo_stock ?? 0));
                    break;
                case "stock_desc":
                    inventario = inventario.OrderByDescending(i => i.cantidad - (i.Materiales.minimo_stock ?? 0));
                    break;
                case "proveedor_asc":
                    inventario = inventario.OrderBy(i => i.Materiales.proveedor);
                    break;
                case "proveedor_desc":
                    inventario = inventario.OrderByDescending(i => i.Materiales.proveedor);
                    break;
                default:
                    inventario = inventario.OrderBy(i => i.Materiales.nombre);
                    break;
            }

            #endregion

            // Pasar los proveedores disponibles a la vista para el dropdown
            ViewBag.Proveedores = db.Materiales
                                .Where(m => m.proveedor != null)
                                .Select(m => m.proveedor)
                                .Distinct()
                                .OrderBy(p => p)
                                .ToList();

            return View(inventario.ToList());
        }

        // GET: Inventarios/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            return View(inventario);
        }

        // GET: Inventarios/Create
        public ActionResult Create()
        {
            ViewBag.id_material = new SelectList(db.Materiales, "id_material", "nombre");
            return View();
        }

        // POST: Inventarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_inventario,id_material,cantidad,fecha_actualizacion")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                // Establecer la fecha de actualización como la fecha actual
                inventario.fecha_actualizacion = DateTime.Today;

                db.Inventario.Add(inventario);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_material = new SelectList(db.Materiales, "id_material", "nombre", inventario.id_material);
            return View(inventario);
        }

        // GET: Inventarios/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_material = new SelectList(db.Materiales, "id_material", "nombre", inventario.id_material);
            return View(inventario);
        }

        // POST: Inventarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_inventario,id_material,cantidad,fecha_actualizacion")] Inventario inventario)
        {
            if (ModelState.IsValid)
            {
                // Actualizar la fecha de actualización
                inventario.fecha_actualizacion = DateTime.Today;

                db.Entry(inventario).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_material = new SelectList(db.Materiales, "id_material", "nombre", inventario.id_material);
            return View(inventario);
        }

        // GET: Inventarios/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Inventario inventario = db.Inventario.Find(id);
            if (inventario == null)
            {
                return HttpNotFound();
            }
            return View(inventario);
        }

        // POST: Inventarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Inventario inventario = db.Inventario.Find(id);
            db.Inventario.Remove(inventario);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Método para obtener sugerencias de búsqueda (autocomplete)
        public JsonResult GetSearchSuggestions(string term, string filterBy)
        {
            var suggestions = new List<string>();

            if (!string.IsNullOrEmpty(term) && !string.IsNullOrEmpty(filterBy))
            {
                term = term.ToLower();
                switch (filterBy)
                {
                    case "nombre":
                        suggestions = db.Materiales
                            .Where(m => m.nombre.ToLower().Contains(term))
                            .Select(m => m.nombre)
                            .Distinct()
                            .Take(10)
                            .ToList();
                        break;
                    case "descripcion":
                        suggestions = db.Materiales
                            .Where(m => m.descripcion.ToLower().Contains(term))
                            .Select(m => m.descripcion)
                            .Distinct()
                            .Take(10)
                            .ToList();
                        break;
                    case "proveedor":
                        suggestions = db.Materiales
                            .Where(m => m.proveedor.ToLower().Contains(term))
                            .Select(m => m.proveedor)
                            .Distinct()
                            .Take(10)
                            .ToList();
                        break;
                }
            }

            return Json(suggestions, JsonRequestBehavior.AllowGet);
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