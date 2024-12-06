using System;
using System.Collections.Generic;
using System.IO;

namespace JpCommon
{
    public static class JpPaths
    {
        public static List<string> ListSubPaths(string folderPath)
        {
            var subfolders = new List<string>();

            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                try
                {
                    DirectoryInfo[] subDirectories = new DirectoryInfo(folderPath).GetDirectories();
                    foreach (var subDir in subDirectories)
                    {
                        subfolders.Add(subDir.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al listar subcarpetas en '{folderPath}': {ex.Message}");
                }
            }

            return subfolders;
        }

        public static string GetMainDrive()
        {
            // Obtiene todos los discos disponibles en el sistema
            var drives = DriveInfo.GetDrives();

            // Busca el disco principal con el sistema operativo instalado
            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    return drive.Name; // Devuelve la unidad principal (por ejemplo, "C:\")
                }
            }

            // Si no encuentra una unidad válida, devuelve una cadena vacía
            return string.Empty;
        }

        public static List<string> GetAllFixedDrives()
        {
            var fixedDrives = new List<string>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    fixedDrives.Add(drive.Name);
                }
            }

            return fixedDrives;
        }

        public static List<string> ListAvailableBackupPaths()
        {
            var fixedDrives = GetAllFixedDrives();
            var allSubfolders = new List<string>();

            foreach (var fixedDrive in fixedDrives)
            {
                var subfoldersInDrive = ListSubPaths(fixedDrive);
                allSubfolders.AddRange(subfoldersInDrive);
            }

            return allSubfolders;
        }

        public static string CreateTemporaryPath()
        {
            string tempFolderPath = Path.GetTempPath();
            string uniqueId = Guid.NewGuid().ToString("N");
            string temporaryPath = Path.Combine(tempFolderPath, uniqueId);

            try
            {
                Directory.CreateDirectory(temporaryPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear el directorio temporal: {ex.Message}");
                throw;
            }

            return temporaryPath;
        }
    }
}


