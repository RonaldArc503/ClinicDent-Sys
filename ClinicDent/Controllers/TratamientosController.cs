using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClinicDent.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ClinicDent.Controllers
{
    public class TratamientosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();
        private const int PageSize = 10; // Número de tratamientos por página

        // GET: Tratamientos
        public ActionResult Index(string searchString, string tipoCobroFilter, string seguimientoFilter,
                                 DateTime? fechaInicio, DateTime? fechaFin, int page = 1)
        {
            // Obtener tipos de cobro para el dropdown
            ViewBag.TiposCobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre");

            // Consulta base
            var tratamientos = db.Tratamientos.Include(t => t.Tipo_Cobro).AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower();
                tratamientos = tratamientos.Where(t =>
                    t.Tipo_Cobro.nombre.ToLower().Contains(searchString) ||
                    t.dientes_seleccionados.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(tipoCobroFilter) && int.TryParse(tipoCobroFilter, out int tipoCobroId))
            {
                tratamientos = tratamientos.Where(t => t.id_tipo_cobro == tipoCobroId);
            }

            if (!string.IsNullOrEmpty(seguimientoFilter) && bool.TryParse(seguimientoFilter, out bool requiereSeguimiento))
            {
                tratamientos = tratamientos.Where(t => t.seguimiento == requiereSeguimiento);
            }

            if (fechaInicio.HasValue)
            {
                tratamientos = tratamientos.Where(t => t.fecha_inicio >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                tratamientos = tratamientos.Where(t => t.fecha_inicio <= fechaFin.Value);
            }

            // Ordenar
            tratamientos = tratamientos.OrderByDescending(t => t.fecha_inicio);

            // Calcular paginación
            int totalRecords = tratamientos.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            // Aplicar paginación
            var tratamientosPaginados = tratamientos
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // Pasar parámetros de filtro y paginación a la vista
            ViewBag.CurrentFilter = searchString;
            ViewBag.TipoCobroFilter = tipoCobroFilter;
            ViewBag.SeguimientoFilter = seguimientoFilter;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(tratamientosPaginados);
        }

        // GET: Tratamientos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Deshabilitar proxies dinámicos para esta consulta
            var tratamiento = db.Tratamientos
                .Include(t => t.Tipo_Cobro)
                .AsNoTracking()
                .FirstOrDefault(t => t.id_tratamiento == id);

            if (tratamiento == null)
            {
                return HttpNotFound();
            }

            // Forzar la carga del nombre si es necesario
            ViewBag.NombreTratamiento = tratamiento.Tipo_Cobro?.nombre;

            return View(tratamiento);
        }

        // GET: Tratamientos/Create
        public ActionResult Create()
        {
            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre");

            ViewBag.FechaActual = DateTime.Now.ToString("yyyy-MM-dd");

            // Inicializar con odontograma vacío
            var tratamiento = new Tratamientos
            {
                fecha_inicio = DateTime.Now,
                seguimiento = false,
                dientes_seleccionados = "{}" // JSON vacío
            };

            return View(tratamiento);
        }

        // POST: Tratamientos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo_cobro,fecha_inicio,costo,duracion_estimada,seguimiento,dientes_seleccionados")] Tratamientos tratamiento)
        {
            ValidarTratamiento(tratamiento);

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar y limpiar el JSON de dientes seleccionados
                    tratamiento.dientes_seleccionados = LimpiarJsonDientes(tratamiento.dientes_seleccionados);

                    db.Tratamientos.Add(tratamiento);
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Tratamiento creado exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error al crear tratamiento: {ex.Message}");
                    ModelState.AddModelError("", "No se pudo guardar el tratamiento. Por favor, intenta de nuevo.");
                }
            }

            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre",
                tratamiento.id_tipo_cobro);

            return View(tratamiento);
        }

        // GET: Tratamientos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Explicitly fetch treatment without tracking to avoid proxy issues
            Tratamientos tratamiento = db.Tratamientos
                .AsNoTracking()
                .FirstOrDefault(t => t.id_tratamiento == id);

            if (tratamiento == null)
            {
                return HttpNotFound();
            }

            // Set default values for nullable or invalid fields
            if (tratamiento.fecha_inicio == default(DateTime))
            {
                tratamiento.fecha_inicio = DateTime.Now;
            }
            if (tratamiento.costo == 0)
            {
                tratamiento.costo = 0.00m;
            }
            if (tratamiento.duracion_estimada == 0)
            {
                tratamiento.duracion_estimada = 1;
            }
            if (string.IsNullOrEmpty(tratamiento.dientes_seleccionados))
            {
                tratamiento.dientes_seleccionados = "{}";
            }

            // Set dropdown for tipo_cobro
            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre",
                tratamiento.id_tipo_cobro);

            return View(tratamiento);
        }

        // POST: Tratamientos/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_tratamiento,id_tipo_cobro,fecha_inicio,costo,duracion_estimada,seguimiento,dientes_seleccionados")] Tratamientos tratamiento)
        {
            ValidarTratamiento(tratamiento);

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar y limpiar el JSON de dientes seleccionados
                    tratamiento.dientes_seleccionados = LimpiarJsonDientes(tratamiento.dientes_seleccionados);

                    db.Entry(tratamiento).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Tratamiento actualizado exitosamente.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Trace.TraceError($"Error al actualizar tratamiento {tratamiento.id_tratamiento}: {ex.Message}");
                    ModelState.AddModelError("", "No se pudo actualizar el tratamiento. Por favor, intenta de nuevo.");
                }
            }

            // Reload dropdown in case of validation failure
            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre",
                tratamiento.id_tipo_cobro);

            return View(tratamiento);
        }

        // GET: Tratamientos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tratamientos tratamiento = db.Tratamientos
                .Include(t => t.Tipo_Cobro)
                .FirstOrDefault(t => t.id_tratamiento == id);
            if (tratamiento == null)
            {
                return HttpNotFound();
            }
            return View(tratamiento);
        }

        // POST: Tratamientos/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Tratamientos tratamiento = db.Tratamientos.Find(id);
                if (tratamiento == null)
                {
                    return HttpNotFound();
                }
                db.Tratamientos.Remove(tratamiento);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Tratamiento eliminado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error al eliminar tratamiento {id}: {ex.Message}");
                TempData["ErrorMessage"] = "No se pudo eliminar el tratamiento. Por favor, intenta de nuevo.";
                return RedirectToAction("Delete", new { id });
            }
        }

        // Método para validar el tratamiento
        private void ValidarTratamiento(Tratamientos tratamiento)
        {
            if (tratamiento.costo <= 0)
            {
                ModelState.AddModelError("costo", "El costo debe ser mayor a cero.");
            }

            if (tratamiento.duracion_estimada <= 0)
            {
                ModelState.AddModelError("duracion_estimada", "La duración estimada debe ser mayor a cero.");
            }

            if (tratamiento.id_tipo_cobro <= 0)
            {
                ModelState.AddModelError("id_tipo_cobro", "Debe seleccionar un tipo de cobro válido.");
            }

            if (tratamiento.fecha_inicio == default(DateTime) || tratamiento.fecha_inicio < DateTime.Now.AddYears(-1))
            {
                ModelState.AddModelError("fecha_inicio", "La fecha de inicio no es válida.");
            }

            // Validar dientes_seleccionados
            if (string.IsNullOrWhiteSpace(tratamiento.dientes_seleccionados))
            {
                ModelState.AddModelError("dientes_seleccionados", "Debe seleccionar al menos un diente o parte en el odontograma.");
                return;
            }

            try
            {
                var json = JObject.Parse(tratamiento.dientes_seleccionados);
                if (!json.Properties().Any())
                {
                    ModelState.AddModelError("dientes_seleccionados", "Debe seleccionar al menos un diente en el odontograma.");
                    return;
                }

                // Validar IDs de dientes y estructura
                var validDientes = new HashSet<string>(Enumerable.Range(11, 8)
                    .Concat(Enumerable.Range(21, 8))
                    .Concat(Enumerable.Range(31, 8))
                    .Concat(Enumerable.Range(41, 8))
                    .Concat(Enumerable.Range(51, 5))
                    .Concat(Enumerable.Range(61, 5))
                    .Concat(Enumerable.Range(71, 5))
                    .Concat(Enumerable.Range(81, 5))
                    .Select(x => x.ToString()));

                var validPartes = new HashSet<string> { "superior", "inferior", "izquierda", "derecha", "centro" };
                var validTratamientos = new HashSet<string> { "caries", "relleno" };
                var validMarcas = new HashSet<string> { "faltante", "extraer" };

                foreach (var diente in json.Properties())
                {
                    if (!validDientes.Contains(diente.Name))
                    {
                        ModelState.AddModelError("dientes_seleccionados", $"ID de diente inválido: {diente.Name}.");
                        continue;
                    }

                    var partes = diente.Value["partes"] as JObject;
                    var marca = diente.Value["marca"]?.ToString();

                    if (partes != null)
                    {
                        foreach (var parte in partes.Properties())
                        {
                            if (!validPartes.Contains(parte.Name))
                            {
                                ModelState.AddModelError("dientes_seleccionados", $"Parte inválida en diente {diente.Name}: {parte.Name}.");
                            }
                            if (!validTratamientos.Contains(parte.Value.ToString()))
                            {
                                ModelState.AddModelError("dientes_seleccionados", $"Tratamiento inválido en diente {diente.Name}, parte {parte.Name}: {parte.Value}.");
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(marca) && !validMarcas.Contains(marca))
                    {
                        ModelState.AddModelError("dientes_seleccionados", $"Marca inválida en diente {diente.Name}: {marca}.");
                    }
                }
            }
            catch (JsonReaderException)
            {
                ModelState.AddModelError("dientes_seleccionados", "El formato de los dientes seleccionados no es válido.");
            }
        }

        // Método para limpiar y validar el JSON de dientes seleccionados
        private string LimpiarJsonDientes(string jsonDientes)
        {
            if (string.IsNullOrWhiteSpace(jsonDientes))
            {
                return "{}";
            }

            try
            {
                var jsonObj = JObject.Parse(jsonDientes);
                var dientesValidos = new JObject();

                // Lista de dientes válidos
                var validDientes = new HashSet<string>(Enumerable.Range(11, 8)
                    .Concat(Enumerable.Range(21, 8))
                    .Concat(Enumerable.Range(31, 8))
                    .Concat(Enumerable.Range(41, 8))
                    .Concat(Enumerable.Range(51, 5))
                    .Concat(Enumerable.Range(61, 5))
                    .Concat(Enumerable.Range(71, 5))
                    .Concat(Enumerable.Range(81, 5))
                    .Select(x => x.ToString()));

                foreach (var diente in jsonObj.Properties())
                {
                    if (!validDientes.Contains(diente.Name))
                    {
                        continue; // Saltar dientes inválidos
                    }

                    var partesValidas = new JObject();
                    var partes = diente.Value["partes"] as JObject;
                    var marca = diente.Value["marca"]?.ToString();

                    if (partes != null)
                    {
                        foreach (var parte in partes.Properties())
                        {
                            if (parte.Name == "superior" || parte.Name == "inferior" ||
                                parte.Name == "izquierda" || parte.Name == "derecha" ||
                                parte.Name == "centro")
                            {
                                if (parte.Value.ToString() == "caries" || parte.Value.ToString() == "relleno")
                                {
                                    partesValidas[parte.Name] = parte.Value.ToString();
                                }
                            }
                        }
                    }

                    var nuevoDiente = new JObject();
                    if (partesValidas.HasValues)
                    {
                        nuevoDiente["partes"] = partesValidas;
                    }

                    if (marca == "faltante" || marca == "extraer")
                    {
                        nuevoDiente["marca"] = marca;
                    }

                    if (nuevoDiente.HasValues)
                    {
                        dientesValidos[diente.Name] = nuevoDiente;
                    }
                }

                return dientesValidos.ToString(Formatting.None);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error al limpiar JSON de dientes: {ex.Message}");
                return "{}";
            }
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