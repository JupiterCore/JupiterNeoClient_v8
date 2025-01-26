using JpCommon;
using System.Text;

namespace JupiterNeoServiceClient.Utils
{
    public static class Logger
    {
        private static readonly string logFilePath;
        private static readonly long maxLogFileSize = 1048576 * 5; // Tamaño máximo del archivo de log en bytes (e.g., 5 MB)

        static Logger()
        {
            string logFileName = "log.txt"; // Nombre del archivo de log

            var filesManager = new JpFilesManager();

            logFilePath = Path.Combine(filesManager.DiskPath!,JpConstants.ClientFolderName, logFileName);

            Console.WriteLine("---------------------------");
            Console.WriteLine(logFilePath);
            Console.WriteLine("---------------------------");
        }

        public static void Log(Exception? ex, string level)
        {
            if (ex != null)
            {
                WriteExceptionLog(ex, level);
            }
            else
            {
                WriteLogToFile(logFilePath, level);
            }
        }

        public static void Log(string log)
        {
            WriteLogToFile(logFilePath, log);
        }

        private static void WriteExceptionLog(Exception ex, string level)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Level: [{level}]");
            messageBuilder.AppendLine("An exception occurred:");
            messageBuilder.AppendLine($"Message: {ex.Message}");
            messageBuilder.AppendLine("Stack Trace:");
            messageBuilder.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                messageBuilder.AppendLine("Inner Exception:");
                messageBuilder.AppendLine($"Message: {ex.InnerException.Message}");
                messageBuilder.AppendLine("Stack Trace:");
                messageBuilder.AppendLine(ex.InnerException.StackTrace);
            }

            WriteLogToFile(logFilePath, messageBuilder.ToString());
        }

        private static void WriteLogToFile(string filePath, string logMessage)
        {
            try
            {
                // Archivar el archivo si supera el tamaño máximo
                if (File.Exists(filePath) && new FileInfo(filePath).Length >= maxLogFileSize)
                {
                    ArchiveLogFile(filePath);
                }

                // Escribir el log en el archivo
                using var sw = new StreamWriter(filePath, append: true, encoding: Encoding.UTF8);
                sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {logMessage}");
            }
            catch (Exception ex)
            {
                // Manejar errores de log opcionalmente
                Console.Error.WriteLine($"Error al escribir en el log: {ex.Message}");
            }
        }

        private static void ArchiveLogFile(string filePath)
        {
            try
            {
                string archiveFileName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMdd_HHmmss}.log";
                string archiveFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, archiveFileName);
                File.Move(filePath, archiveFilePath);
            }
            catch (Exception ex)
            {
                // Manejar errores durante el archivado del log
                Console.Error.WriteLine($"Error al archivar el archivo de log: {ex.Message}");
            }
        }
    }
}
