using System;
using System.IO;

namespace JupiterNeoServiceClient.Utils
{
    public class FileProcessor
    {
        // Método para verificar si un archivo puede ser leído
        public static bool FileCanBeRead(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                // Si la ruta del archivo está vacía o nula, retornamos false.
                return false;
            }

            try
            {
                // Intentamos abrir el archivo en modo lectura.
                using (var fileStream = File.OpenRead(filePath))
                {
                    // Si no se lanza una excepción, el archivo puede ser leído.
                    return true;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Si no tenemos permisos para acceder al archivo.
                return false;
            }
            catch (FileNotFoundException)
            {
                // Si el archivo no se encuentra.
                return false;
            }
            catch (IOException)
            {
                // Otras excepciones de I/O, como el archivo está siendo utilizado por otro proceso.
                return false;
            }
            catch (Exception)
            {
                // Captura cualquier otro error inesperado y retorna false.
                return false;
            }
        }
    }
}
