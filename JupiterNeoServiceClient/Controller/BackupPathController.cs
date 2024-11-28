using JupiterNeoServiceClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Controllers
{
    public class BackupPathController
    {
        private readonly BackupPathModel _backupPathModel;

        // Constructor con inyección de dependencias
        public BackupPathController(BackupPathModel backupPathModel)
        {
            _backupPathModel = backupPathModel ?? throw new ArgumentNullException(nameof(backupPathModel));
        }

        public void UpdatePaths(string[] newPaths)
        {
            if (newPaths == null || newPaths.Length == 0)
                throw new ArgumentException("No paths provided for update.");

            var currentPaths = _backupPathModel.GetAllPaths();
            List<string> currentPathsList = currentPaths.Select(p => p.bapa_path).ToList();

            // Agregar nuevas rutas que no existen localmente
            foreach (string path in newPaths)
            {
                if (!_backupPathModel.ExistsPath(path))
                {
                    _backupPathModel.AddPath(path);
                }
            }

            // Eliminar rutas locales que ya no están en las nuevas rutas
            foreach (string path in currentPathsList)
            {
                if (!newPaths.Contains(path))
                {
                    _backupPathModel.DeleteByPath(path);
                }
            }
        }
    }
}

