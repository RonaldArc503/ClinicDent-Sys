using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

public class ConfiguracionesController : Controller
{
    // GET: Configuraciones
    public ActionResult Index()
    {
        return View();
    }

  
    public ActionResult ExportDatabase()
    {
        try
        {
            // Verificar conexión a la base de datos primero
            string connectionString = ConfigurationManager.ConnectionStrings["ClinicaDentalLocal0Sql"].ConnectionString;

            // Verificar si la cadena de conexión existe
            if (string.IsNullOrEmpty(connectionString))
            {
                TempData["ErrorMessage"] = "No se encontró la cadena de conexión en web.config";
                return RedirectToAction("Index");
            }

            // Verificar permisos de escritura
            string backupDir = Server.MapPath("~/App_Data/Backups");
            try
            {
                Directory.CreateDirectory(backupDir);
                // Probar si podemos escribir en el directorio
                string testFile = Path.Combine(backupDir, "test.txt");
                System.IO.File.WriteAllText(testFile, "test");
                System.IO.File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "La aplicación no tiene permisos para escribir en el directorio de backups";
                return RedirectToAction("Index");
            }

            string backupFileName = $"ClinicaDental_Backup_{DateTime.Now:yyyyMMddHHmmss}.bak";
            string backupPath = Path.Combine(backupDir, backupFileName);

            // Usar parámetros SQL para mayor seguridad
            string backupQuery = @"BACKUP DATABASE [ClinicaDental1] 
                              TO DISK = @backupPath 
                              WITH COMPRESSION, FORMAT, 
                              MEDIANAME = 'SQLServerBackups', 
                              NAME = 'Full Backup of ClinicaDental1';";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(backupQuery, connection))
                {
                    command.Parameters.AddWithValue("@backupPath", backupPath);

                    try
                    {
                        connection.Open();
                        int result = command.ExecuteNonQuery();

                        // Verificar si el backup se creó
                        if (!System.IO.File.Exists(backupPath))
                        {
                            TempData["ErrorMessage"] = "El comando se ejecutó pero no se creó el archivo de backup";
                            return RedirectToAction("Index");
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        TempData["ErrorMessage"] = $"Error de SQL: {sqlEx.Message}";
                        return RedirectToAction("Index");
                    }
                }
            }

            // Verificar tamaño del archivo
            FileInfo backupFile = new FileInfo(backupPath);
            if (backupFile.Length == 0)
            {
                TempData["ErrorMessage"] = "El archivo de backup se creó pero está vacío";
                System.IO.File.Delete(backupPath);
                return RedirectToAction("Index");
            }

            // Descargar el archivo
            byte[] fileBytes = System.IO.File.ReadAllBytes(backupPath);
            TempData["SuccessMessage"] = $"Backup creado exitosamente. Tamaño: {backupFile.Length / 1024 / 1024} MB";

            return File(fileBytes, "application/octet-stream", backupFileName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"Error completo: {ex.ToString()}");
            TempData["ErrorMessage"] = $"Error completo al exportar: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
    [Authorize(Roles = "Administrador")]
    public ActionResult ListarBackups()
    {
        try
        {
            string backupDir = Server.MapPath("~/App_Data/Backups");
            Directory.CreateDirectory(backupDir); // Asegura que el directorio exista

            var backups = Directory.GetFiles(backupDir, "*.bak")
                                 .Select(f => new FileInfo(f))
                                 .OrderByDescending(f => f.CreationTime)
                                 .ToList();

            return View(backups);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"Error al listar backups: {ex}");
            TempData["ErrorMessage"] = "Error al listar los backups existentes";
            return RedirectToAction("Index");
        }
    }

    [Authorize(Roles = "Administrador")]
    public ActionResult DescargarBackup(string fileName)
    {
        try
        {
            string backupPath = Path.Combine(Server.MapPath("~/App_Data/Backups"), fileName);

            if (!System.IO.File.Exists(backupPath))
            {
                TempData["ErrorMessage"] = "El archivo de backup solicitado no existe";
                return RedirectToAction("ListarBackups");
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(backupPath);
            return File(fileBytes, "application/octet-stream", fileName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"Error al descargar backup: {ex}");
            TempData["ErrorMessage"] = "Error al descargar el backup";
            return RedirectToAction("ListarBackups");
        }
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public ActionResult EliminarBackup(string fileName)
    {
        try
        {
            string backupPath = Path.Combine(Server.MapPath("~/App_Data/Backups"), fileName);

            if (System.IO.File.Exists(backupPath))
            {
                System.IO.File.Delete(backupPath);
                TempData["SuccessMessage"] = "Backup eliminado correctamente";
            }

            return RedirectToAction("ListarBackups");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"Error al eliminar backup: {ex}");
            TempData["ErrorMessage"] = "Error al eliminar el backup";
            return RedirectToAction("ListarBackups");
        }
    }
}