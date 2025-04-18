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
    public class ConsultasController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        // GET: Consultas
        public ActionResult Index()
        {
            var consulta = db.Consulta.Include(c => c.Citas).Include(c => c.Dentistas).Include(c => c.Pacientes);
            return View(consulta.ToList());
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
        // GET: Consultas/Create
public ActionResult Create(int? idCita, string fechaConsulta, int? idDentista, int? idPaciente)
{
    // Inicializar listas desplegables
    ViewBag.id_cita = new SelectList(db.Citas, "id_cita", "estado");
    ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre");
    ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres");

    var model = new Consulta();

    if (idCita != null)
    {
        var cita = db.Citas.Find(idCita);
        if (cita != null)
        {
            model.id_cita = cita.id_cita;
            model.fecha_consulta = cita.fecha_hora; // Asignar fecha directamente desde la cita
            
            // Configurar ViewBag para mostrar la fecha formateada
            ViewBag.FechaFormateada = cita.fecha_hora.ToString("dd MMM. yyyy HH:mm", 
                                      new System.Globalization.CultureInfo("es-ES"));
            
            if (idDentista != null)
            {
                model.id_dentista = idDentista.Value;
                ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", idDentista);
            }

            if (idPaciente != null)
            {
                model.id_paciente = idPaciente.Value;
                ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", idPaciente);
            }
        }
    }

    return View(model);
}

        // POST: Consultas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_consulta,id_cita,id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            if (ModelState.IsValid)
            {
                db.Consulta.Add(consulta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Reconstruir listas desplegables si hay error de validación
            ViewBag.id_cita = new SelectList(db.Citas, "id_cita", "estado", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);

            return View(consulta);
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
            ViewBag.id_cita = new SelectList(db.Citas, "id_cita", "estado", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);
            return View(consulta);
        }

        // POST: Consultas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_consulta,id_cita,id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            if (ModelState.IsValid)
            {
                db.Entry(consulta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id_cita = new SelectList(db.Citas, "id_cita", "estado", consulta.id_cita);
            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);
            return View(consulta);
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // GET: Consultas/consulta_sin_cita
public ActionResult Consulta_sin_cita()
{
    ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre");
    ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres");
    
    // Establecer fecha actual por defecto
    var model = new Consulta
    {
        fecha_consulta = DateTime.Now
    };
    
    return View(model);
}

// POST: Consultas/consulta_sin_cita
/*
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Consulta_sin_cita([Bind(Include = "id_dentista,id_paciente,fecha_consulta,diagnostico,observaciones,recomendaciones,requiere_tratamiento")] Consulta consulta)
        {
            if (ModelState.IsValid)
            {
                // Asegurar explícitamente que id_cita sea NULL
                consulta.id_cita = null;

                db.Consulta.Add(consulta);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.id_dentista = new SelectList(db.Dentistas, "id_dentista", "nombre", consulta.id_dentista);
            ViewBag.id_paciente = new SelectList(db.Pacientes, "id_paciente", "nombres", consulta.id_paciente);

            return View(consulta);
        }*/
    }
}