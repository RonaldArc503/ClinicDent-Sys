using ClinicDent.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClinicDent.Controllers
{
    public class TratamientosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();
        private const int PageSize = 10;

        public ActionResult Index(string searchString, string tipoCobroFilter, string seguimientoFilter,
                                 DateTime? fechaInicio, DateTime? fechaFin, int page = 1)
        {
            ViewBag.TiposCobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre");

            var tratamientos = db.Tratamientos.Include(t => t.Tipo_Cobro).AsQueryable();

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

            tratamientos = tratamientos.OrderByDescending(t => t.fecha_inicio);

            int totalRecords = tratamientos.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            page = Math.Max(1, Math.Min(page, totalPages));

            var tratamientosPaginados = tratamientos
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            ViewBag.CurrentFilter = searchString;
            ViewBag.TipoCobroFilter = tipoCobroFilter;
            ViewBag.SeguimientoFilter = seguimientoFilter;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(tratamientosPaginados);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tratamiento = db.Tratamientos
                .Include(t => t.Tipo_Cobro)
                .AsNoTracking()
                .FirstOrDefault(t => t.id_tratamiento == id);

            if (tratamiento == null)
            {
                return HttpNotFound();
            }

            ViewBag.NombreTratamiento = tratamiento.Tipo_Cobro?.nombre;

            return View(tratamiento);
        }

        public ActionResult Create()
        {
            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre");

            ViewBag.FechaActual = DateTime.Now.ToString("yyyy-MM-dd");

            var tratamiento = new Tratamientos
            {
                fecha_inicio = DateTime.Now,
                seguimiento = false,
                dientes_seleccionados = "{}"
            };

            return View(tratamiento);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_tipo_cobro,fecha_inicio,costo,duracion_estimada,seguimiento,dientes_seleccionados")] Tratamientos tratamiento)
        {
            ValidarTratamiento(tratamiento);

            if (ModelState.IsValid)
            {
                try
                {
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

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Tratamientos tratamiento = db.Tratamientos
                .AsNoTracking()
                .FirstOrDefault(t => t.id_tratamiento == id);

            if (tratamiento == null)
            {
                return HttpNotFound();
            }

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

            ViewBag.id_tipo_cobro = new SelectList(
                db.Tipo_Cobro.OrderBy(t => t.nombre),
                "id_tipo_cobro",
                "nombre",
                tratamiento.id_tipo_cobro);

            return View(tratamiento);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tratamientos model)
        {
            ValidarTratamiento(model); // Reuse the existing validation method

            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Datos inválidos. Por favor, revisa los campos." });
                }
                ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro.OrderBy(t => t.nombre), "id_tipo_cobro", "nombre", model.id_tipo_cobro);
                return View(model);
            }

            var tratamiento = db.Tratamientos.Find(model.id_tratamiento);
            if (tratamiento == null)
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Tratamiento no encontrado." });
                }
                return HttpNotFound();
            }

            try
            {
                tratamiento.id_tipo_cobro = model.id_tipo_cobro;
                tratamiento.fecha_inicio = model.fecha_inicio;
                tratamiento.costo = model.costo;
                tratamiento.duracion_estimada = model.duracion_estimada;
                tratamiento.seguimiento = model.seguimiento;
                tratamiento.dientes_seleccionados = LimpiarJsonDientes(model.dientes_seleccionados);

                db.Entry(tratamiento).State = EntityState.Modified;
                db.SaveChanges();

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, message = "Tratamiento editado exitosamente." });
                }

                TempData["SuccessMessage"] = "Tratamiento editado exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Error al actualizar tratamiento {model.id_tratamiento}: {ex.Message}");
                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = false, message = $"No se pudo actualizar el tratamiento: {ex.Message}" });
                }
                TempData["ErrorMessage"] = "No se pudo actualizar el tratamiento. Por favor, intenta de nuevo.";
                ViewBag.id_tipo_cobro = new SelectList(db.Tipo_Cobro.OrderBy(t => t.nombre), "id_tipo_cobro", "nombre", model.id_tipo_cobro);
                return View(model);
            }
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var tratamiento = await db.Tratamientos
                .Include(t => t.Tipo_Cobro)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.id_tratamiento == id);

            if (tratamiento == null)
            {
                return HttpNotFound();
            }

            ViewBag.NombreTratamiento = tratamiento.Tipo_Cobro?.nombre;
            return View(tratamiento);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tratamiento = await db.Tratamientos.FindAsync(id);
                if (tratamiento == null)
                {
                    TempData["ErrorMessage"] = "El tratamiento no fue encontrado.";
                    return RedirectToAction("Index");
                }

                db.Tratamientos.Remove(tratamiento);
                await db.SaveChangesAsync();
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

            if (string.IsNullOrWhiteSpace(tratamiento.dientes_seleccionados))
            {
                ModelState.AddModelError("dientes_seleccionados", "Debe seleccionar al menos un diente o parte en el odontograma.");
                return;
            }

            try
            {
                var json = JObject.Parse(tratamiento.dientes_seleccionados);
                if (!json.Properties().Any(p => p.Name != "puentes"))
                {
                    ModelState.AddModelError("dientes_seleccionados", "Debe seleccionar al menos un diente en el odontograma.");
                    return;
                }

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
                var validMarcas = new HashSet<string> { "faltante", "extraer", "necesita-endodoncia", "tiene-endodoncia", "puente-azul", "puente-rojo" };

                foreach (var diente in json.Properties())
                {
                    if (diente.Name == "puentes")
                    {
                        var puentes = diente.Value as JArray;
                        if (puentes != null)
                        {
                            foreach (var puente in puentes)
                            {
                                var dienteId1 = puente["dienteId1"]?.ToString();
                                var dienteId2 = puente["dienteId2"]?.ToString();
                                var color = puente["color"]?.ToString();
                                if (!validDientes.Contains(dienteId1) || !validDientes.Contains(dienteId2))
                                {
                                    ModelState.AddModelError("dientes_seleccionados", $"ID de diente inválido en puente: {dienteId1}, {dienteId2}.");
                                }
                                if (color != "azul" && color != "roja")
                                {
                                    ModelState.AddModelError("dientes_seleccionados", $"Color de puente inválido: {color}.");
                                }
                            }
                        }
                        continue;
                    }

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
            catch (JsonReaderException ex)
            {
                Trace.TraceError($"Error parsing dientes_seleccionados: {ex.Message}");
                ModelState.AddModelError("dientes_seleccionados", "El formato de los dientes seleccionados no es válido.");
            }
        }

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

                var validDientes = new HashSet<string>(Enumerable.Range(11, 8)
                    .Concat(Enumerable.Range(21, 8))
                    .Concat(Enumerable.Range(31, 8))
                    .Concat(Enumerable.Range(41, 8))
                    .Concat(Enumerable.Range(51, 5))
                    .Concat(Enumerable.Range(61, 5))
                    .Concat(Enumerable.Range(71, 5))
                    .Concat(Enumerable.Range(81, 5))
                    .Select(x => x.ToString()));

                var validMarcas = new HashSet<string> { "faltante", "extraer", "necesita-endodoncia", "tiene-endodoncia", "puente-azul", "puente-rojo" };

                foreach (var diente in jsonObj.Properties())
                {
                    if (diente.Name == "puentes")
                    {
                        var puentes = diente.Value as JArray;
                        if (puentes != null)
                        {
                            var validPuentes = new JArray();
                            foreach (var puente in puentes)
                            {
                                var dienteId1 = puente["dienteId1"]?.ToString();
                                var dienteId2 = puente["dienteId2"]?.ToString();
                                var color = puente["color"]?.ToString();
                                if (validDientes.Contains(dienteId1) && validDientes.Contains(dienteId2) && (color == "azul" || color == "roja"))
                                {
                                    validPuentes.Add(puente);
                                }
                            }
                            if (validPuentes.Any())
                            {
                                dientesValidos["puentes"] = validPuentes;
                            }
                        }
                        continue;
                    }

                    if (!validDientes.Contains(diente.Name))
                    {
                        continue;
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

                    if (!string.IsNullOrEmpty(marca) && validMarcas.Contains(marca))
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