using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ClinicDent.Models;

namespace ClinicDent.Controllers
{
    public class PagosController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Pagos
        public ActionResult Index()
        {
            var pagos = db.Database.SqlQuery<PagoViewModel>("SELECT * FROM VistaPagos").ToList();
            return View(pagos);
        }

        // GET: Pagos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pago = db.Database.SqlQuery<PagoViewModel>("SELECT * FROM VistaPagos WHERE id_pago = @id", new SqlParameter("@id", id)).FirstOrDefault();
            if (pago == null)
            {
                return HttpNotFound();
            }
            return View(pago);
        }

        // GET: Pagos/Create
        public ActionResult Create()
        {
            ViewBag.IdConsulta = new SelectList(db.Consulta, "id_consulta", "diagnostico");
            ViewBag.MetodoPago = new SelectList(new[] { "Efectivo", "Tarjeta", "Transferencia" });
            ViewBag.TipoPago = new SelectList(new[] { "Unico", "Cuotas" });
            return View(new PagoCreateViewModel());
        }

        // POST: Pagos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PagoCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = db.Database.Connection)
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "RegistrarPago";
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add(new SqlParameter("@id_consulta", model.IdConsulta));
                            command.Parameters.Add(new SqlParameter("@fecha_pago", model.FechaPago));
                            command.Parameters.Add(new SqlParameter("@monto_total", model.MontoTotal));
                            command.Parameters.Add(new SqlParameter("@metodo_pago", model.MetodoPago));
                            command.Parameters.Add(new SqlParameter("@tipo_pago", model.TipoPago));

                            // Crear tabla para cuotas
                            var cuotasTable = new DataTable();
                            cuotasTable.Columns.Add("fecha_inicio", typeof(DateTime));
                            cuotasTable.Columns.Add("costo", typeof(decimal));
                            // Columnas adicionales de TratamientoType (no usadas, pero requeridas)
                            cuotasTable.Columns.Add("id_tipo_cobro", typeof(int));
                            cuotasTable.Columns.Add("duracion_estimada", typeof(int));
                            cuotasTable.Columns.Add("seguimiento", typeof(bool));
                            cuotasTable.Columns.Add("dientes_seleccionados", typeof(string));
                            cuotasTable.Columns.Add("cantidad", typeof(int));
                            cuotasTable.Columns.Add("total", typeof(decimal));

                            if (model.Cuotas != null && model.Cuotas.Any())
                            {
                                foreach (var cuota in model.Cuotas)
                                {
                                    cuotasTable.Rows.Add(
                                        cuota.FechaInicio,
                                        cuota.Costo,
                                        null, // id_tipo_cobro
                                        null, // duracion_estimada
                                        null, // seguimiento
                                        null, // dientes_seleccionados
                                        null, // cantidad
                                        null  // total
                                    );
                                }
                            }

                            var cuotasParam = new SqlParameter("@cuotas", SqlDbType.Structured)
                            {
                                TypeName = "dbo.TratamientoType",
                                Value = cuotasTable
                            };
                            command.Parameters.Add(cuotasParam);

                            var idPago = (int)command.ExecuteScalar();
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message} (Código: {ex.Number})");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.IdConsulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", model.IdConsulta);
            ViewBag.MetodoPago = new SelectList(new[] { "Efectivo", "Tarjeta", "Transferencia" }, model.MetodoPago);
            ViewBag.TipoPago = new SelectList(new[] { "Unico", "Cuotas" }, model.TipoPago);
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
            ViewBag.IdConsulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", pago.id_consulta);
            return View(pago);
        }

        // POST: Pagos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_pago,id_consulta,fecha_pago,monto_total,metodo_pago,tipo_pago")] Pagos pago)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pago).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdConsulta = new SelectList(db.Consulta, "id_consulta", "diagnostico", pago.id_consulta);
            return View(pago);
        }

        // GET: Pagos/UpdateCuota/5
        public ActionResult UpdateCuota(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var cuota = db.Database.SqlQuery<Pagos_Cuotas>("SELECT * FROM Pagos_Cuotas WHERE id_cuota = @id", new SqlParameter("@id", id)).FirstOrDefault();
            if (cuota == null)
            {
                return HttpNotFound();
            }
            var model = new ActualizarCuotaViewModel
            {
                IdCuota = cuota.id_cuota,
                Estado = cuota.estado,
                FechaPago = cuota.fecha_pago
            };
            ViewBag.Estado = new SelectList(new[] { "Pendiente", "Pagada", "Vencida" }, cuota.estado);
            return View(model);
        }

        // POST: Pagos/UpdateCuota/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCuota(ActualizarCuotaViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = db.Database.Connection)
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = "ActualizarCuota";
                            command.CommandType = CommandType.StoredProcedure;

                            command.Parameters.Add(new SqlParameter("@id_cuota", model.IdCuota));
                            command.Parameters.Add(new SqlParameter("@estado", model.Estado));
                            command.Parameters.Add(new SqlParameter("@fecha_pago", model.FechaPago));

                            command.ExecuteScalar();
                            return RedirectToAction("Index");
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message} (Código: {ex.Number})");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.Estado = new SelectList(new[] { "Pendiente", "Pagada", "Vencida" }, model.Estado);
            return View(model);
        }

        // GET: Pagos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pago = db.Database.SqlQuery<PagoViewModel>("SELECT * FROM VistaPagos WHERE id_pago = @id", new SqlParameter("@id", id)).FirstOrDefault();
            if (pago == null)
            {
                return HttpNotFound();
            }
            return View(pago);
        }

        // POST: Pagos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Pagos pago = db.Pagos.Find(id);
            db.Pagos.Remove(pago);
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