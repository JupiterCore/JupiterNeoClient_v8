using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace JupiterNeoServiceClient.classes
{
    public static class Helpers
    {
        public static string Today()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static List<string> DirSearch(string sDir, string[] extensions)
        {
            var files = new List<string>();
            try
            {
                // Buscar archivos en el directorio actual que coincidan con las extensiones.
                files.AddRange(Directory.EnumerateFiles(sDir)
                    .Where(f => extensions.Contains(Path.GetExtension(f))));

                // Procesar subdirectorios recursivamente.
                foreach (string d in Directory.EnumerateDirectories(sDir))
                {
                    if (IsExcludedFolder(d))
                        continue;

                    files.AddRange(DirSearch(d, extensions));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Manejar carpetas sin permisos de acceso.
                // Opcional: Loggear el acceso denegado si se implementa un sistema de logging.
            }
            catch (Exception ex)
            {
                // Manejar errores generales.
                // Opcional: Loggear o registrar errores.
                Console.WriteLine($"Error in DirSearch: {ex.Message}");
            }

            return files;
        }

        private static bool IsExcludedFolder(string folderPath)
        {
            string folderName = Path.GetFileName(folderPath);

            // Excluir carpetas con nombres específicos.
            if (folderName.StartsWith("~") || folderName.StartsWith("."))
                return true;

            // Comparar rutas de carpetas específicas que no deben incluirse.
            string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Temp");
            if (string.Equals(folderPath, tempFolderPath, StringComparison.OrdinalIgnoreCase))
                return true;

            if (folderPath.Contains(@"Local\Temp") || folderPath.Contains(@"LocalLow\Temp"))
                return true;

            string cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cache");
            if (string.Equals(folderPath, cacheFolderPath, StringComparison.OrdinalIgnoreCase))
                return true;

            string localAppDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.Equals(folderPath, localAppDataFolderPath, StringComparison.OrdinalIgnoreCase))
                return true;

            string roamingAppDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (string.Equals(folderPath, roamingAppDataFolderPath, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public static bool HasTimeElapsed(string targetTime)
        {
            if (!TimeSpan.TryParse(targetTime, out TimeSpan target))
                throw new ArgumentException("Invalid target time format.", nameof(targetTime));

            return DateTime.Now.TimeOfDay >= target;
        }

        public static bool canPingServer()
        {
            try
            {
                using Ping myPing = new Ping();
                string host = Environment.GetEnvironmentVariable("JUPITER_API_HOST") ??
#if DEBUG
                    "http://localhost:8080";
#else
                    "https://api.jupiterneo.cloud";
#endif
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();

                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                // Opcional: loggear el error o registrar información del fallo.
                Console.WriteLine($"Ping error: {ex.Message}");
                return false;
            }
        }
    }
}
