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
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Usuarios model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Convertir a lista en memoria primero para evitar el problema de tipos
                    var usuario = db.Usuarios
                        .Include("Roles")
                        .AsEnumerable() // Esto trae los datos a memoria
                        .FirstOrDefault(u => u.usuario == model.usuario && u.clave == model.clave);

                    if (usuario != null)
                    {
                        // Crear ticket de autenticación con el rol
                        string userData = usuario.Roles?.nombre;

                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                            1,
                            usuario.usuario,
                            DateTime.Now,
                            DateTime.Now.AddMinutes(30),
                            false,
                            userData,
                            FormsAuthentication.FormsCookiePath);

                        string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                        Response.Cookies.Add(authCookie);

                        return RedirectToLocal(returnUrl);
                    }

                    ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                }
                catch (Exception ex)
                {
                    // Registrar el error
                    System.Diagnostics.Trace.TraceError($"Error en Login: {ex}");
                    ModelState.AddModelError("", "Ocurrió un error al intentar iniciar sesión.");
                }
            }

            return View(model);
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Account");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}