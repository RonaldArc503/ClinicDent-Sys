using ClinicDent.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ClinicDent.Controllers
{
    public class AccountController : Controller
    {
        private ClinicaDentalLocal0 db = new ClinicaDentalLocal0();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Usuarios model)
        {
            if (ModelState.IsValid)
            {
                // Buscar el usuario en la base de datos
                var usuario = db.Usuarios
      .AsEnumerable() // Bring data to memory
      .FirstOrDefault(u => u.usuario == model.usuario && u.clave == model.clave);

                if (usuario != null)
                {
                    // Autenticar al usuario
                    FormsAuthentication.SetAuthCookie(model.usuario, false);
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
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