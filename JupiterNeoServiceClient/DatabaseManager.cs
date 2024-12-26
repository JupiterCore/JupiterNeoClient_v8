using JpCommon;
using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.IO;
using System.Reflection;

namespace JupiterNeoServiceClient
{
    public class DatabaseManager
    {
        private MetadataModel _metadataModel = new MetadataModel(false);
        private JpFilesManager filesManager = new JpFilesManager();
        private FileManager fm = new FileManager();

        public DatabaseManager()
        {
        }

        private bool canConnectToNewDatabase()
        {
            bool canConnect = false;
            try
            {
                // Conexión a la base de datos nueva
                this._metadataModel.SetDatabaseConnection(true);
                this._metadataModel.getBackupId(); // Verifica si la conexión es válida
                canConnect = true;
            }
            catch (Exception)
            {
                canConnect = false;
                try
                {
                    BaseModel.Disconnect();
                }
                catch (Exception) { }
            }
            return canConnect;
        }

        public void updateDatabaseIfNeeded()
        {
            try
            {
                bool canConnectToNewDatabase = this.canConnectToNewDatabase();
                if (!this.isNewDatabaseFound() || !canConnectToNewDatabase)
                {
                    this.updateDatabase();
                }
                this.RecoverLicense();
            }
            catch (Exception ex)
            {
                Logger.Log($"[updateDatabase] {ex.Message}");
            }
        }

        private bool isNewDatabaseFound()
        {
            var applicationPath = this.fm.AppContainer;
            var newDbName = Path.Combine(applicationPath, JpConstants.SQLiteNameNeo);
            return File.Exists(newDbName);
        }

        private void updateDatabase()
        {
            string tmpPath = JpPaths.CreateTemporaryPath();
            try
            {
                // Extraer recurso incrustado
                Assembly assembly = Assembly.GetExecutingAssembly();
                JpResourceExtractor.ExtractEmbeddedResource(assembly, JpConstants.SQLiteNameNeo, tmpPath);

                var fullTmpPath = Path.Combine(tmpPath, JpConstants.SQLiteNameNeo);
                if (File.Exists(fullTmpPath))
                {
                    // Verificar si la carpeta de destino existe
                    if (!Directory.Exists(fm.AppContainer))
                    {
                        Directory.CreateDirectory(fm.AppContainer);
                    }

                    // Copiar archivo, sobrescribiendo si ya existe
                    var fullFinalPath = Path.Combine(fm.AppContainer, JpConstants.SQLiteNameNeo);
                    File.Copy(fullTmpPath, fullFinalPath, true);
                }
                else
                {
                    throw new FileNotFoundException($"The file {fullTmpPath} was not found.");
                }
            }
            finally
            {
                // Eliminar la carpeta temporal, si existe
                if (Directory.Exists(tmpPath))
                {
                    Directory.Delete(tmpPath, true);
                }
            }
        }

        private void RecoverLicense()
        {
            this._metadataModel.SetDatabaseConnection(false);
            string license = this._metadataModel.getLicense();
            if (license != null && license.Length > 0)
            {
                this._metadataModel.SetDatabaseConnection(true);
                string currentLicense = this._metadataModel.getLicense();
                if (currentLicense.Length <= 0)
                {
                    this._metadataModel.insertLicense(license);
                }
            }
        }
    }
}
