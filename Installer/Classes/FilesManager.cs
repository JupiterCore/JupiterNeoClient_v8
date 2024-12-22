/****
 * WARNING
 * 
 * All the code form CommonClassessLibrary needs to be moved to JpCommon
 * After all the code has been removed from this project you have to delete it
 * 
 * WARNING
 * 
**/

using System.Text;

namespace CommonClasessLibrary
{

    public class FilesManager
    {
        public string? diskPath { get; set; }
        public string? installationPath { get; set; }
        public string? exePath { get; set; }
        public string? installBatPath { get; set; }
        public string? removeBatPath { get; set; }
        public string? versionFilePath { get; set; }
        public string? zipPath { get; set; }

        public FilesManager()
        {
            getMainDisk();
            setInstallationPath();
            setVersionFilePath();
        }

        public void setInstallationPath()
        {
            if (diskPath != null)
            {
                // Previously: Constants.JP_FOLDER
                installationPath = Path.Combine(diskPath, "JupiterNeoClient");
            }
        }

        public void setBatPaths()
        {
            if (installationPath != null)
            {
                installBatPath = Path.Combine(installationPath, "install.bat");
                removeBatPath = Path.Combine(installationPath, "remove.bat");
            }
        }

        public void getMainDisk()
        {
            diskPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        public void setVersionFilePath()
        {
            if (installationPath != null)
            {
                versionFilePath = Path.Combine(installationPath, "version.txt");
            }
        }

        public string getCurrentVersionInstalled()
        {
            string result = "";
            if (File.Exists(versionFilePath))
            {
                string[] lines = File.ReadAllLines(versionFilePath);
                if (lines.Length > 0)
                {
                    result = lines[0];
                }
            }
            return result;
        }

        public bool writeNewVersion(string newVersion)
        {
            bool wasCreated = false;
            if (versionFilePath != null)
            {
                if (File.Exists(versionFilePath))
                {
                    File.Delete(versionFilePath);
                }

                using (FileStream fs = File.Create(versionFilePath))
                {
                    byte[] jpVersion = new UTF8Encoding(true).GetBytes(newVersion);
                    fs.Write(jpVersion, 0, jpVersion.Length);
                }
                wasCreated = File.Exists(versionFilePath);
            }
            else
            {
                // No se ha podido crear el nuevo archivo de versión
            }
            return wasCreated;
        }

        public void setZipPath(string zipName)
        {
            if (diskPath != null)
            {
                zipPath = Path.Join(diskPath, zipName);
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
            else
            {
                return null;
            }
        }
    }
}
