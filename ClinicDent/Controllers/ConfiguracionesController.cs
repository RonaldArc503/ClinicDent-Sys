using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace ClinicDent.Controllers
{
    public class ConfiguracionesController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ExportDatabase()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ClinicaDentalLocal0Sql"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    TempData["ErrorMessage"] = "No se encontró la configuración de conexión a la base de datos";
                    return RedirectToAction("Index");
                }

                string backupDir = Server.MapPath("~/App_Data/Backups");
                Directory.CreateDirectory(backupDir);

                // Nombre fijo para el archivo de backup
                string backupFileName = "ClinicaDental_Backup.bak";
                string backupPath = Path.Combine(backupDir, backupFileName);

                // Eliminar el archivo anterior si existe
                if (System.IO.File.Exists(backupPath))
                {
                    System.IO.File.Delete(backupPath);
                }

                // Verificar permisos de escritura
                try
                {
                    string testFile = Path.Combine(backupDir, "test.txt");
                    System.IO.File.WriteAllText(testFile, "test");
                    System.IO.File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["ErrorMessage"] = "La aplicación no tiene permisos para escribir en el directorio de backups";
                    return RedirectToAction("Index");
                }

                // Crear el backup
                string backupQuery = @"BACKUP DATABASE [ClinicaDental1] 
                                    TO DISK = @backupPath
                                    WITH COMPRESSION, FORMAT,
                                    MEDIANAME = 'ClinicaDentalBackups',
                                    NAME = 'Backup Completo de ClinicaDental';";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(backupQuery, connection))
                    {
                        command.Parameters.AddWithValue("@backupPath", backupPath);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                // Verificar que el backup se creó correctamente
                if (!System.IO.File.Exists(backupPath) || new FileInfo(backupPath).Length == 0)
                {
                    TempData["ErrorMessage"] = "El backup no se creó correctamente";
                    return RedirectToAction("Index");
                }

                // Preparar la descarga del archivo
                byte[] fileBytes = System.IO.File.ReadAllBytes(backupPath);
                string contentType = "application/octet-stream";

                TempData["SuccessMessage"] = $"Backup actualizado exitosamente ({new FileInfo(backupPath).Length / 1024 / 1024} MB)";

                // Devolver el archivo para descarga
                return File(fileBytes, contentType, backupFileName);
            }
            catch (SqlException sqlEx)
            {
                TempData["ErrorMessage"] = $"Error de SQL Server: {sqlEx.Message}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
                return RedirectToAction("Index");
            }
        }


        public ActionResult ExportDatabaseAsSQL()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["ClinicaDentalLocal0Sql"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    TempData["ErrorMessage"] = "No se encontró la configuración de conexión a la base de datos";
                    return RedirectToAction("Index");
                }

                string exportDir = Server.MapPath("~/App_Data/Exports");
                Directory.CreateDirectory(exportDir);

                string exportFileName = $"ClinicaDental_Export_{DateTime.Now:yyyyMMddHHmmss}.sql";
                string exportPath = Path.Combine(exportDir, exportFileName);

                // Verificar permisos de escritura
                try
                {
                    string testFile = Path.Combine(exportDir, "test.txt");
                    System.IO.File.WriteAllText(testFile, "test");
                    System.IO.File.Delete(testFile);
                }
                catch (UnauthorizedAccessException)
                {
                    TempData["ErrorMessage"] = "La aplicación no tiene permisos para escribir en el directorio de exports";
                    return RedirectToAction("Index");
                }

                // Obtener todas las tablas
                var tables = new List<string>();
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    DataTable schema = connection.GetSchema("Tables");
                    foreach (DataRow row in schema.Rows)
                    {
                        tables.Add(row["TABLE_NAME"].ToString());
                    }
                }

                // Generar script SQL
                using (var writer = new StreamWriter(exportPath))
                {
                    writer.WriteLine("-- Exportación SQL de ClinicaDental");
                    writer.WriteLine("-- Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    writer.WriteLine("SET IDENTITY_INSERT ON;");
                    writer.WriteLine("BEGIN TRANSACTION;");
                    writer.WriteLine();

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        foreach (var table in tables)
                        {
                            writer.WriteLine($"-- Datos de la tabla {table}");
                            writer.WriteLine($"DELETE FROM [{table}]; -- Elimina datos existentes para reemplazar");
                            writer.WriteLine();

                            // Obtener estructura de la tabla (columnas)
                            var columns = new List<string>();
                            using (var command = new SqlCommand($"SELECT TOP 0 * FROM [{table}]", connection))
                            using (var reader = command.ExecuteReader())
                            {
                                var schemaTable = reader.GetSchemaTable();
                                foreach (DataRow row in schemaTable.Rows)
                                {
                                    columns.Add(row["ColumnName"].ToString());
                                }
                            }

                            // Obtener datos de la tabla
                            using (var command = new SqlCommand($"SELECT * FROM [{table}]", connection))
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var columnNames = string.Join(", ", columns.Select(c => $"[{c}]"));
                                    var values = new List<string>();

                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        if (reader.IsDBNull(i))
                                        {
                                            values.Add("NULL");
                                        }
                                        else
                                        {
                                            var value = reader.GetValue(i);
                                            if (value is string || value is DateTime || value is Guid)
                                            {
                                                values.Add($"'{value.ToString().Replace("'", "''")}'");
                                            }
                                            else if (value is bool)
                                            {
                                                values.Add((bool)value ? "1" : "0");
                                            }
                                            else
                                            {
                                                values.Add(value.ToString());
                                            }
                                        }
                                    }

                                    var valueList = string.Join(", ", values);
                                    writer.WriteLine($"INSERT INTO [{table}] ({columnNames}) VALUES ({valueList});");
                                }
                            }
                            writer.WriteLine();
                        }
                    }

                    writer.WriteLine("COMMIT TRANSACTION;");
                    writer.WriteLine("SET IDENTITY_INSERT OFF;");
                }

                if (!System.IO.File.Exists(exportPath) || new FileInfo(exportPath).Length == 0)
                {
                    TempData["ErrorMessage"] = "La exportación no se creó correctamente";
                    return RedirectToAction("Index");
                }

                byte[] fileBytes = System.IO.File.ReadAllBytes(exportPath);
                string contentType = "application/sql";

                TempData["SuccessMessage"] = $"Exportación SQL creada exitosamente ({new FileInfo(exportPath).Length / 1024} KB)";

                return File(fileBytes, contentType, exportFileName);
            }
            catch (SqlException sqlEx)
            {
                TempData["ErrorMessage"] = $"Error de SQL Server: {sqlEx.Message}";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error inesperado: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}