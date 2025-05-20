using ClinicDent.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ClinicDent.Controllers
{
    public class PagosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Pagos
        public ActionResult Index()
        {
            var pagos = db.Pagos.Select(p => new PagoViewModel
            {
                IdPago = p.id_pago,
                IdConsulta = p.id_consulta,
                FechaPago = p.fecha_pago,
                MontoTotal = p.monto_total,
                MetodoPago = p.metodo_pago,
                TipoPago = p.tipo_pago,
                Cuotas = p.Pagos_Cuotas.Select(c => new PagoCuotaViewModel
                {
                    IdCuota = c.id_cuota,
                    IdPago = c.id_pago,
                    FechaPago = c.fecha_pago,
                    Monto = c.monto,
                    Estado = c.estado
                }).ToList()
            }).ToList();

            return View(pagos);
        }

        // GET: Pagos/Create
        public ActionResult Create()
        {
            var model = new PagoCreateViewModel
            {
                Consultas = GetConsultasSelectList(),
                MetodosPago = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
                    new SelectListItem { Value = "Tarjeta", Text = "Tarjeta" },
                    new SelectListItem { Value = "Transferencia", Text = "Transferencia" }
                },
                TiposPago = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Unico", Text = "Único" },
                    new SelectListItem { Value = "Cuotas", Text = "Cuotas" }
                }
            };

            Debug.WriteLine($"Consultas cargadas: {model.Consultas.Count()}");
            return View(model);
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PagoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Validate consultation exists
                    if (!db.Consulta.Any(c => c.id_consulta == model.IdConsulta))
                    {
                        ModelState.AddModelError("IdConsulta", "La consulta seleccionada no existe.");
                    }

                    // Validate cuotas for Cuotas type
                    if (model.TipoPago == "Cuotas")
                    {
                        if (model.NumeroCuotas <= 0)
                        {
                            ModelState.AddModelError("NumeroCuotas", "El número de cuotas debe ser mayor que 0.");
                        }
                        else if (model.Cuotas == null || model.Cuotas.Count == 0)
                        {
                            // Auto-generate cuotas if not provided
                            model.Cuotas = new List<ClinicDent.Models.CuotaInputModel>();
                            decimal montoPorCuota = model.MontoTotal / model.NumeroCuotas;
                            for (int i = 0; i < model.NumeroCuotas; i++)
                            {
                                model.Cuotas.Add(new ClinicDent.Models.CuotaInputModel
                                {
                                    FechaPago = DateTime.Now.AddMonths(i),
                                    Monto = montoPorCuota,
                                    Estado = "Pendiente"
                                });
                            }
                        }

                        // Validate cuotas count and sum
                        if (model.Cuotas.Count != model.NumeroCuotas)
                        {
                            ModelState.AddModelError("Cuotas", $"Debe proporcionar exactamente {model.NumeroCuotas} cuotas.");
                        }

                        decimal cuotasSum = model.Cuotas.Sum(c => c.Monto);
                        if (Math.Abs(cuotasSum - model.MontoTotal) > 0.01m)
                        {
                            ModelState.AddModelError("Cuotas", "La suma de los montos de las cuotas debe coincidir con el monto total.");
                        }
                    }
                    else
                    {
                        model.Cuotas = new List<ClinicDent.Models.CuotaInputModel>(); // Ensure empty cuotas for Unico
                    }

                    if (ModelState.IsValid)
                    {
                        // Create DataTable for cuotas
                        DataTable cuotasTable = new DataTable();
                        cuotasTable.Columns.Add("fecha_pago", typeof(DateTime));
                        cuotasTable.Columns.Add("monto", typeof(decimal));
                        cuotasTable.Columns.Add("estado", typeof(string));

                        foreach (var cuota in model.Cuotas)
                        {
                            cuotasTable.Rows.Add(cuota.FechaPago, cuota.Monto, cuota.Estado);
                        }

                        // Call stored procedure
                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("dbo.GestionarPago", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;

                                command.Parameters.AddWithValue("@id_consulta", model.IdConsulta);
                                command.Parameters.AddWithValue("@fecha_pago", DateTime.Now);
                                command.Parameters.AddWithValue("@monto_total", model.MontoTotal);
                                command.Parameters.AddWithValue("@metodo_pago", model.MetodoPago ?? "Efectivo");
                                command.Parameters.AddWithValue("@tipo_pago", model.TipoPago ?? "Unico");
                                command.Parameters.AddWithValue("@fecha_registro", DateTime.Today);

                                var cuotasParam = command.Parameters.AddWithValue("@cuotas", cuotasTable);
                                cuotasParam.SqlDbType = SqlDbType.Structured;
                                cuotasParam.TypeName = "dbo.CuotasType";

                                var result = command.ExecuteScalar();
                                Debug.WriteLine($"Pago creado con ID: {result}");
                            }
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Error en la base de datos: {ex.Message}");
                Debug.WriteLine($"SQL Excepción: {ex.Message}\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar el pago: {ex.Message}");
                Debug.WriteLine($"Excepción: {ex.Message}\n{ex.StackTrace}");
            }

            // Reload dropdowns
            model.Consultas = GetConsultasSelectList();
            model.MetodosPago = new List<SelectListItem>
            {
                new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
                new SelectListItem { Value = "Tarjeta", Text = "Tarjeta" },
                new SelectListItem { Value = "Transferencia", Text = "Transferencia" }
            };
            model.TiposPago = new List<SelectListItem>
            {
                new SelectListItem { Value = "Unico", Text = "Único" },
                new SelectListItem { Value = "Cuotas", Text = "Cuotas" }
            };

            return View(model);
        }

        // GET: Pagos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Pagos pago = db.Pagos.Find(id);
            if (pago == null)
            {
                return HttpNotFound();
            }

            var model = new PagoCreateViewModel
            {
                IdConsulta = pago.id_consulta ?? 0,
                MontoTotal = pago.monto_total,
                MetodoPago = pago.metodo_pago,
                TipoPago = pago.tipo_pago,
                NumeroCuotas = pago.Pagos_Cuotas.Count,
                Cuotas = pago.Pagos_Cuotas.Select(c => new ClinicDent.Models.CuotaInputModel
                {
                    FechaPago = c.fecha_pago,
                    Monto = c.monto,
                    Estado = c.estado
                }).ToList(),
                Consultas = GetConsultasSelectList(),
                MetodosPago = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
                    new SelectListItem { Value = "Tarjeta", Text = "Tarjeta" },
                    new SelectListItem { Value = "Transferencia", Text = "Transferencia" }
                },
                TiposPago = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Unico", Text = "Único" },
                    new SelectListItem { Value = "Cuotas", Text = "Cuotas" }
                }
            };

            return View(model);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, PagoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    // Validate consultation exists
                    if (!db.Consulta.Any(c => c.id_consulta == model.IdConsulta))
                    {
                        ModelState.AddModelError("IdConsulta", "La consulta seleccionada no existe.");
                    }

                    // Validate cuotas for Cuotas type
                    if (model.TipoPago == "Cuotas")
                    {
                        if (model.NumeroCuotas <= 0)
                        {
                            ModelState.AddModelError("NumeroCuotas", "El número de cuotas debe ser mayor que 0.");
                        }
                        else if (model.Cuotas == null || model.Cuotas.Count == 0)
                        {
                            // Auto-generate cuotas if not provided
                            model.Cuotas = new List<ClinicDent.Models.CuotaInputModel>();
                            decimal montoPorCuota = model.MontoTotal / model.NumeroCuotas;
                            for (int i = 0; i < model.NumeroCuotas; i++)
                            {
                                model.Cuotas.Add(new ClinicDent.Models.CuotaInputModel
                                {
                                    FechaPago = DateTime.Now.AddMonths(i),
                                    Monto = montoPorCuota,
                                    Estado = "Pendiente"
                                });
                            }
                        }

                        // Validate cuotas count and sum
                        if (model.Cuotas.Count != model.NumeroCuotas)
                        {
                            ModelState.AddModelError("Cuotas", $"Debe proporcionar exactamente {model.NumeroCuotas} cuotas.");
                        }

                        decimal cuotasSum = model.Cuotas.Sum(c => c.Monto);
                        if (Math.Abs(cuotasSum - model.MontoTotal) > 0.01m)
                        {
                            ModelState.AddModelError("Cuotas", "La suma de los montos de las cuotas debe coincidir con el monto total.");
                        }
                    }
                    else
                    {
                        model.Cuotas = new List<ClinicDent.Models.CuotaInputModel>();
                    }

                    if (ModelState.IsValid)
                    {
                        // Delete existing payment and cuotas
                        var pago = db.Pagos.Find(id);
                        if (pago != null)
                        {
                            db.Pagos_Cuotas.RemoveRange(pago.Pagos_Cuotas);
                            db.Pagos.Remove(pago);
                            db.SaveChanges();
                        }

                        // Create new payment using GestionarPago
                        DataTable cuotasTable = new DataTable();
                        cuotasTable.Columns.Add("fecha_pago", typeof(DateTime));
                        cuotasTable.Columns.Add("monto", typeof(decimal));
                        cuotasTable.Columns.Add("estado", typeof(string));

                        foreach (var cuota in model.Cuotas)
                        {
                            cuotasTable.Rows.Add(cuota.FechaPago, cuota.Monto, cuota.Estado);
                        }

                        using (var connection = new SqlConnection(db.Database.Connection.ConnectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("dbo.GestionarPago", connection))
                            {
                                command.CommandType = CommandType.StoredProcedure;

                                command.Parameters.AddWithValue("@id_consulta", model.IdConsulta);
                                command.Parameters.AddWithValue("@fecha_pago", DateTime.Now);
                                command.Parameters.AddWithValue("@monto_total", model.MontoTotal);
                                command.Parameters.AddWithValue("@metodo_pago", model.MetodoPago ?? "Efectivo");
                                command.Parameters.AddWithValue("@tipo_pago", model.TipoPago ?? "Unico");
                                command.Parameters.AddWithValue("@fecha_registro", DateTime.Today);

                                var cuotasParam = command.Parameters.AddWithValue("@cuotas", cuotasTable);
                                cuotasParam.SqlDbType = SqlDbType.Structured;
                                cuotasParam.TypeName = "dbo.CuotasType";

                                var result = command.ExecuteScalar();
                                Debug.WriteLine($"Pago actualizado con ID: {result}");
                            }
                        }

                        return RedirectToAction("Index");
                    }
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError("", $"Error en la base de datos: {ex.Message}");
                Debug.WriteLine($"SQL Excepción: {ex.Message}\n{ex.StackTrace}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar el pago: {ex.Message}");
                Debug.WriteLine($"Excepción: {ex.Message}\n{ex.StackTrace}");
            }

            model.Consultas = GetConsultasSelectList();
            model.MetodosPago = new List<SelectListItem>
    {
        new SelectListItem { Value = "Efectivo", Text = "Efectivo" },
        new SelectListItem { Value = "Tarjeta", Text = "Tarjeta" },
        new SelectListItem { Value = "Transferencia", Text = "Transferencia" }
    };
            model.TiposPago = new List<SelectListItem>
    {
        new SelectListItem { Value = "Unico", Text = "Único" },
        new SelectListItem { Value = "Cuotas", Text = "Cuotas" }
    };

            return View(model);
        }

        // GET: Pagos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Pagos pago = db.Pagos.Find(id);
            if (pago == null)
            {
                return HttpNotFound();
            }

            var model = new PagoViewModel
            {
                IdPago = pago.id_pago,
                IdConsulta = pago.id_consulta,
                FechaPago = pago.fecha_pago,
                MontoTotal = pago.monto_total,
                MetodoPago = pago.metodo_pago,
                TipoPago = pago.tipo_pago,
                Cuotas = pago.Pagos_Cuotas.Select(c => new PagoCuotaViewModel
                {
                    IdCuota = c.id_cuota,
                    IdPago = c.id_pago,
                    FechaPago = c.fecha_pago,
                    Monto = c.monto,
                    Estado = c.estado
                }).ToList()
            };

            return View(model);
        }

        public class CustomDateTimeBinder : IModelBinder
        {
            public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
            {
                var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                if (value != null)
                {
                    try
                    {
                        return DateTime.ParseExact(value.AttemptedValue, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, "Formato de fecha inválido (dd/mm/aaaa)");
                    }
                }
                return null;
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                Pagos pago = db.Pagos
                    .Include(p => p.Pagos_Cuotas)
                    .Include(p => p.Historial_Pagos) // Añade esto para cargar el historial
                    .FirstOrDefault(p => p.id_pago == id);

                if (pago == null)
                {
                    TempData["Error"] = "Pago no encontrado.";
                    return RedirectToAction("Index");
                }

                // Eliminar primero las cuotas si existen
                if (pago.Pagos_Cuotas.Any())
                {
                    db.Pagos_Cuotas.RemoveRange(pago.Pagos_Cuotas);
                }

                // Eliminar el historial de pagos si existe
                if (pago.Historial_Pagos.Any())
                {
                    db.Historial_Pagos.RemoveRange(pago.Historial_Pagos);
                }

                // Luego eliminar el pago
                db.Pagos.Remove(pago);

                db.SaveChanges();
                TempData["Success"] = "Pago eliminado correctamente.";
            }
            catch (DbUpdateException dbEx)
            {
                var sqlEx = dbEx.GetBaseException() as SqlException;
                if (sqlEx != null && (sqlEx.Number == 547))
                {
                    TempData["Error"] = "No se puede eliminar el pago porque está relacionado con otros registros.";
                }
                else
                {
                    TempData["Error"] = "Error al eliminar el pago: " + dbEx.Message;
                }
                Debug.WriteLine($"Error al eliminar: {dbEx.Message}\n{dbEx.StackTrace}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción al eliminar: {ex.Message}\n{ex.StackTrace}");
                TempData["Error"] = "Error al eliminar el pago: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper method to get consultas dropdown
        private IEnumerable<SelectListItem> GetConsultasSelectList()
        {
            var consultas = db.Consulta
                .GroupJoin(db.Pacientes,
                    c => c.id_paciente,
                    p => p.id_paciente,
                    (c, p) => new { c, p = p.DefaultIfEmpty() })
                .SelectMany(x => x.p.DefaultIfEmpty(), (c, p) => new
                {
                    c.c.id_consulta,
                    c.c.fecha_consulta,
                    NombrePaciente = p != null ? p.nombres + " " + p.apellidos : "Sin paciente"
                })
                .ToList();

            return consultas.Select(c => new SelectListItem
            {
                Value = c.id_consulta.ToString(),
                Text = $"Consulta #{c.id_consulta} - {c.fecha_consulta:dd/MM/yyyy} - {c.NombrePaciente}"
            });
        }

        // Helper method to log ModelState errors
        private void LogModelStateErrors()
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Debug.WriteLine($"Error en {state.Key}: {error.ErrorMessage}");
                }
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