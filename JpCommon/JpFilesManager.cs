using System;
using System.IO;
using System.Text;

namespace JpCommon
{
    public class JpFilesManager
    {
        public string? DiskPath { get; set; }
        public string? InstallationPath { get; set; }
        public string? ExePath { get; set; }
        public string? InstallBatPath { get; set; }
        public string? RemoveBatPath { get; set; }
        public string? VersionFilePath { get; set; }
        public string? ZipPath { get; set; }

        public JpFilesManager()
        {
            SetMainDisk();
            SetInstallationPath();
            SetVersionFilePath();
        }

        public void SetInstallationPath()
        {
            if (!string.IsNullOrEmpty(DiskPath))
            {
                InstallationPath = Path.Combine(DiskPath, "JupiterNeoClient");
            }
        }

        public void SetBatPaths()
        {
            if (!string.IsNullOrEmpty(InstallationPath))
            {
                InstallBatPath = Path.Combine(InstallationPath, "install.bat");
                RemoveBatPath = Path.Combine(InstallationPath, "remove.bat");
            }
        }

        public void SetMainDisk()
        {
            DiskPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public void SetVersionFilePath()
        {
            if (!string.IsNullOrEmpty(InstallationPath))
            {
                VersionFilePath = Path.Combine(InstallationPath, "version.txt");
            }
        }

        public string GetCurrentVersionInstalled()
        {
            if (!string.IsNullOrEmpty(VersionFilePath) && File.Exists(VersionFilePath))
            {
                string[] lines = File.ReadAllLines(VersionFilePath);
                return lines.Length > 0 ? lines[0] : string.Empty;
            }

            return string.Empty;
        }

        public bool WriteNewVersion(string newVersion)
        {
            if (!string.IsNullOrEmpty(VersionFilePath))
            {
                try
                {
                    // Eliminar archivo existente, si aplica
                    if (File.Exists(VersionFilePath))
                    {
                        File.Delete(VersionFilePath);
                    }

                    // Crear un nuevo archivo y escribir la versión
                    File.WriteAllText(VersionFilePath, newVersion, Encoding.UTF8);

                    return File.Exists(VersionFilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al escribir la nueva versión: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        public void SetZipPath(string zipName)
        {
            if (!string.IsNullOrEmpty(DiskPath))
            {
                ZipPath = Path.Combine(DiskPath, zipName);
            }
        }

        public string? CopyFileToTemp(string filePath)
        {
            if (File.Exists(filePath))
            {
                string tempPath = Path.GetTempFileName();
                File.Copy(filePath, tempPath, true);
                return tempPath;
            }

            return null;
        }
    }
}

