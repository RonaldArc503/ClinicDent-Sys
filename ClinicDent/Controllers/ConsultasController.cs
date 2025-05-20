using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using ClinicDent.Models;

namespace ClinicDent.Controllers
{
    public class ConsultasController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();
        private readonly string _connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ClinicaDentalLocal0Sql"].ConnectionString;

        // GET: Consultas
        public ActionResult Index(string searchString, string filterBy, string fechaFilter)
        {
            var consultas = db.Consulta
                .Include(c => c.Citas)
                .Include(c => c.Dentistas)
                .Include(c => c.Pacientes)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                switch (filterBy)
                {
                    case "paciente":
                        consultas = consultas.Where(c => c.Pacientes.nombres.ToLower().Contains(searchString));
                        break;
                    case "dentista":
                        consultas = consultas.Where(c => c.Dentistas.nombre.ToLower().Contains(searchString));
                        break;
                    case "diagnostico":
                        consultas = consultas.Where(c => c.diagnostico != null && c.diagnostico.ToLower().Contains(searchString));
                        break;
                    default:
                        consultas = consultas.Where(c =>
                            c.Pacientes.nombres.ToLower().Contains(searchString) ||
                            c.Dentistas.nombre.ToLower().Contains(searchString) ||
                            (c.diagnostico != null && c.diagnostico.ToLower().Contains(searchString)));
                        break;
                }
            }

            if (!String.IsNullOrEmpty(fechaFilter))
            {
                switch (fechaFilter)
                {
                    case "Hoy":
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) == DbFunctions.TruncateTime(DateTime.Today));
                        break;
                    case "Semana":
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(7);
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) >= startOfWeek && DbFunctions.TruncateTime(c.fecha_consulta) < endOfWeek);
                        break;
                    case "Mes":
                        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                        var endOfMonth = startOfMonth.AddMonths(1);
                        consultas = consultas.Where(c => DbFunctions.TruncateTime(c.fecha_consulta) >= startOfMonth && DbFunctions.TruncateTime(c.fecha_consulta) < endOfMonth);
                        break;
                }
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.FilterBy = filterBy;
            ViewBag.FechaFilter = fechaFilter;

            return View(consultas.OrderBy(c => c.fecha_consulta).ToList());
        }

        // GET: Consultas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }
            return View(consulta);
        }

        // GET: Consultas/Create
        public ActionResult Create(int? idCita, string fechaConsulta, int? idDentista, int? idPaciente)
        {
            var model = new ConsultaConTratamientoViewModel();

            // Si viene de una cita, cargar los datos relacionados
            if (idCita.HasValue)
            {
                var cita = db.Citas
                    .Include(c => c.Pacientes)
                    .Include(c => c.Dentistas)
                    .FirstOrDefault(c => c.id_cita == idCita);

                if (cita != null)
                {
                    model.IdCita = cita.id_cita;
                    model.IdPaciente = cita.id_paciente;
                    model.IdDentista = cita.id_dentista;
                    model.FechaConsulta = cita.fecha_hora;

                    ViewBag.PacienteSeleccionado = cita.Pacientes?.nombres;
                    ViewBag.DentistaSeleccionado = cita.Dentistas?.nombre;
                    ViewBag.FechaConsulta = cita.fecha_hora.ToString("dd MMM. yyyy HH:mm", new System.Globalization.CultureInfo("es-ES"));
                }
            }
            else
            {
                model.FechaConsulta = DateTime.Now;
            }

            // Configurar dropdowns
            ViewBag.Citas = new SelectList(db.Citas, "id_cita", "fecha_hora", model.IdCita);
            ViewBag.Dentistas = new SelectList(db.Dentistas, "id_dentista", "nombre", model.IdDentista);
            ViewBag.Pacientes = new SelectList(db.Pacientes, "id_paciente", "nombres", model.IdPaciente);
            ViewBag.TiposCobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ConsultaConTratamientoViewModel model)
        {
            try
            {
                // Validaciones básicas
                if (model.IdCita == 0) model.IdCita = null;

                if (model.IdCita.HasValue && db.Consulta.Any(c => c.id_cita == model.IdCita))
                {
                    ModelState.AddModelError("IdCita", "Esta cita ya tiene una consulta asociada.");
                }

                if (model.FechaConsulta == default(DateTime))
                {
                    ModelState.AddModelError("FechaConsulta", "La fecha de consulta es requerida.");
                }

                // Validación del tratamiento
                if (model.RequiereTratamiento)
                {
                    if (model.Tratamiento == null)
                    {
                        ModelState.AddModelError("", "La información del tratamiento es requerida.");
                    }
                    else
                    {
                        if (model.Tratamiento.IdTipoCobro == 0)
                            ModelState.AddModelError("Tratamiento.IdTipoCobro", "Seleccione un tipo de tratamiento.");
                        if (model.Tratamiento.FechaInicio == default(DateTime))
                            ModelState.AddModelError("Tratamiento.FechaInicio", "La fecha de inicio es requerida.");
                        if (model.Tratamiento.Costo <= 0)
                            ModelState.AddModelError("Tratamiento.Costo", "Ingrese un costo válido.");
                        if (string.IsNullOrEmpty(model.Tratamiento.DientesSeleccionados) || model.Tratamiento.DientesSeleccionados == "{}")
                            ModelState.AddModelError("Tratamiento.DientesSeleccionados", "Seleccione los dientes afectados.");
                        if (model.Tratamiento.Cantidad <= 0)
                            ModelState.AddModelError("Tratamiento.Cantidad", "La cantidad debe ser mayor a 0.");
                        if (model.Tratamiento.Total <= 0)
                            ModelState.AddModelError("Tratamiento.Total", "El total debe ser mayor a 0.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "Errores en el formulario: " + string.Join("; ", errors) });
                }

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Crear tabla de tratamientos para el procedimiento
                    var tratamientosTable = new DataTable();

                    tratamientosTable.Columns.Add("id_tipo_cobro", typeof(int));
                    tratamientosTable.Columns.Add("fecha_inicio", typeof(DateTime));
                    tratamientosTable.Columns.Add("costo", typeof(decimal));
                    tratamientosTable.Columns.Add("duracion_estimada", typeof(int));
                    tratamientosTable.Columns.Add("seguimiento", typeof(bool));
                    tratamientosTable.Columns.Add("dientes_seleccionados", typeof(string));
                    tratamientosTable.Columns.Add("cantidad", typeof(int));
                    tratamientosTable.Columns.Add("total", typeof(decimal));

                    // Agregar el tratamiento al DataTable (si aplica)
                    if (model.RequiereTratamiento && model.Tratamiento != null)
                    {
                        tratamientosTable.Rows.Add(
                            model.Tratamiento.IdTipoCobro,
                            model.Tratamiento.FechaInicio,
                            model.Tratamiento.Costo,
                            model.Tratamiento.DuracionEstimada,
                            model.Tratamiento.Seguimiento,
                            model.Tratamiento.DientesSeleccionados,
                            model.Tratamiento.Cantidad,
                            model.Tratamiento.Total
                        );
                    }

                    using (var command = new SqlCommand("CrearConsultaConTratamientos", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Parámetros de la consulta
                        command.Parameters.AddWithValue("@id_cita", model.IdCita ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@id_dentista", model.IdDentista);
                        command.Parameters.AddWithValue("@id_paciente", model.IdPaciente);
                        command.Parameters.AddWithValue("@fecha_consulta", model.FechaConsulta);
                        command.Parameters.AddWithValue("@diagnostico", model.Diagnostico ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@observaciones", model.Observaciones ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@recomendaciones", model.Recomendaciones ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@requiere_tratamiento", model.RequiereTratamiento);
                        command.Parameters.AddWithValue("@fecha_registro", DateTime.Today);

                        // Parámetro de la tabla de tratamientos
                        var tratamientosParam = command.Parameters.AddWithValue("@tratamientos", tratamientosTable);
                        tratamientosParam.SqlDbType = SqlDbType.Structured;
                        tratamientosParam.TypeName = "dbo.TratamientoType";

                        // Ejecutar el procedimiento y obtener el id_consulta generado
                        var idConsulta = command.ExecuteScalar();

                        // Verificar si se retornó un id_consulta
                        if (idConsulta != null)
                        {
                            return Json(new { success = true, message = "Consulta creada exitosamente con ID: " + idConsulta });
                        }
                        else
                        {
                            return Json(new { success = false, message = "No se pudo obtener el ID de la consulta creada." });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error SQL al crear consulta: {ex.Message}, Número: {ex.Number}");
                string errorMessage = "Ocurrió un error al guardar la consulta.";

                // Manejo de errores específicos
                if (ex.Number == 547) // Violación de clave foránea
                {
                    if (ex.Message.Contains("FK__Consulta__id_pac"))
                        errorMessage = "El paciente seleccionado no existe.";
                    else if (ex.Message.Contains("FK__Consulta__id_den"))
                        errorMessage = "El dentista seleccionado no existe.";
                    else if (ex.Message.Contains("FK__Tratamie__id_tip"))
                        errorMessage = "El tipo de cobro seleccionado no es válido.";
                }

                return Json(new { success = false, message = errorMessage + " Detalles: " + ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general al crear consulta: {ex}");
                return Json(new { success = false, message = "Ocurrió un error inesperado al guardar la consulta. Detalles: " + ex.Message });
            }
        }

        // GET: Consultas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }

            PopulateDropdowns(consulta.id_cita, consulta.id_dentista, consulta.id_paciente);
            return View(consulta);
        }

        // POST: Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_consulta,id_cita,id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            try
            {
                if (consulta.id_cita == null || consulta.id_cita == 0)
                {
                    consulta.id_cita = null;
                }
                else
                {
                    if (db.Consulta.Any(c => c.id_cita == consulta.id_cita && c.id_consulta != consulta.id_consulta))
                    {
                        ModelState.AddModelError("id_cita", "Esta cita ya tiene una consulta asociada.");
                    }
                }

                if (consulta.fecha_consulta == default(DateTime))
                {
                    ModelState.AddModelError("fecha_consulta", "Por favor, especifique una fecha y hora válidas para la consulta.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    PopulateDropdowns(consulta.id_cita, consulta.id_dentista, consulta.id_paciente);
                    return Json(new { success = false, message = "Errores en el formulario: " + string.Join("; ", errors) });
                }

                db.Entry(consulta).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { success = true, message = "Consulta actualizada exitosamente." });
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error SQL al actualizar consulta: {ex.Message}, Número: {ex.Number}");
                string errorMessage = "Ocurrió un error al actualizar la consulta.";

                if (ex.Number == 547) // Violación de clave foránea
                {
                    if (ex.Message.Contains("FK__Consulta__id_pac"))
                        errorMessage = "El paciente seleccionado no existe.";
                    else if (ex.Message.Contains("FK__Consulta__id_den"))
                        errorMessage = "El dentista seleccionado no existe.";
                    else if (ex.Message.Contains("FK__Consulta__id_cit"))
                        errorMessage = "La cita seleccionada no es válida.";
                }

                PopulateDropdowns(consulta.id_cita, consulta.id_dentista, consulta.id_paciente);
                return Json(new { success = false, message = errorMessage + " Detalles: " + ex.Message });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general al actualizar consulta: {ex}");
                PopulateDropdowns(consulta.id_cita, consulta.id_dentista, consulta.id_paciente);
                return Json(new { success = false, message = "Ocurrió un error inesperado al actualizar la consulta. Detalles: " + ex.Message });
            }
        }
        // GET: Consultas/RenderOdontogramaPartial
        public ActionResult RenderOdontogramaPartial(int index)
        {
            ViewBag.Index = index;
            return PartialView("_OdontogramaPartial");
        }

        // GET: Consultas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Consulta consulta = db.Consulta.Find(id);
            if (consulta == null)
            {
                return HttpNotFound();
            }
            return View(consulta);
        }

        // POST: Consultas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Consulta consulta = db.Consulta.Find(id);
            db.Consulta.Remove(consulta);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void PopulateDropdowns(int? idCita = null, int? idDentista = null, int? idPaciente = null)
        {
            var citasPendientesCanceladas = db.Citas
                .Where(c => c.estado == "Pendiente" || c.estado == "Cancelada")
                .Include(c => c.Pacientes)
                .ToList()
                .Select(c => new
                {
                    c.id_cita,
                    Display = $"{c.id_cita} - {c.Pacientes.nombres} - {c.fecha_hora.ToString("dd MMM yyyy HH:mm")}"
                });

            ViewBag.Citas = new SelectList(citasPendientesCanceladas, "id_cita", "Display", idCita);
            ViewBag.Dentistas = new SelectList(db.Dentistas, "id_dentista", "nombre", idDentista);
            ViewBag.Pacientes = new SelectList(db.Pacientes, "id_paciente", "nombres", idPaciente);
            ViewBag.TiposCobro = new SelectList(db.Tipo_Cobro, "id_tipo_cobro", "nombre");
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