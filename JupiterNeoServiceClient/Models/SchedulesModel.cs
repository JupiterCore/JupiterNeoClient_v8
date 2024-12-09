﻿using JpCommon;
using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Controllers
{
    public class SchedulesController : BaseController
    {
        private readonly SchedulesModel _schedulesModel;
        private readonly FileModel _fileModel;
        private readonly BackupPathController _backupPathController;

        public string ScheduleId { get; private set; }
        public DateTime LastTimeScanned { get; private set; } = DateTime.MinValue;
        public bool WasSystemScanned { get; private set; }

        // Constructor que recibe Api como dependencia inyectada.
        public SchedulesController(
            SchedulesModel schedulesModel,
            FileModel fileModel,
            BackupPathController backupPathController,
            MetadataModel metaModel,
            JpApi api // Aquí es donde se inyecta la API
        ) : base(metaModel, api)
        {
            _schedulesModel = schedulesModel ?? throw new ArgumentNullException(nameof(schedulesModel));
            _fileModel = fileModel ?? throw new ArgumentNullException(nameof(fileModel));
            _backupPathController = backupPathController ?? throw new ArgumentNullException(nameof(backupPathController));
        }

        public async Task VerifySchedulesAsync()
        {
            if (!string.IsNullOrEmpty(License))
            {
                var result = await Api.GetSchedulesAsync(License);  // Llamada a la API para obtener los horarios
                if (result?.Schedules != null)
                {
                    foreach (var schedule in result.Schedules)
                    {
                        if (_schedulesModel.GetSchedule(schedule) == null)
                        {
                            _schedulesModel.InsertSchedule(schedule);  // Insertar nuevo horario si no existe
                        }
                    }
                }

                if (result?.Paths != null)
                {
                    _backupPathController.UpdatePaths(result.Paths);  // Actualizar rutas de respaldo
                }
            }
        }

        public async Task<bool> ScanPathAsync(string path)
        {
            var extensions = await Api.GetExtensionsAsync(License);
            if (extensions.Length == 0)
            {
                return false;
            }

            if (!Directory.Exists(path))
            {
                Logger.Log($"Path does not exist: {path}", "SchedulesController");
                return false;
            }

            try
            {
                var filesInDir = Helpers.DirSearch(path, extensions);
                foreach (var file in filesInDir)
                {
                    if (!Directory.Exists(file))
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.Exists)
                        {
                            var fileInDb = _fileModel.FileByPath(file);
                            var lastWrite = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");

                            if (fileInDb == null)
                            {
                                _fileModel.InsertFile(file, fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), lastWrite);
                            }
                            else if (fileInDb.FileUpdatedAt != lastWrite)
                            {
                                _fileModel.OnFileModified(file, lastWrite);
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error scanning path in SchedulesController");
                return false;
            }
        }

        public bool ShouldBackup()
        {
            var uncompleted = _schedulesModel.GetUncompletedBackup();
            if (uncompleted != null)
            {
                ScheduleId = uncompleted.BascId;
                WasSystemScanned = uncompleted.BascScanned == 1;
                return true;
            }

            var unstarted = _schedulesModel.GetAllUnstartedBackups();
            foreach (var backup in unstarted)
            {
                if (Helpers.HasTimeElapsed(backup.BascTime))
                {
                    ScheduleId = backup.BascId;
                    _schedulesModel.MarkScheduleAsStarted(backup);
                    WasSystemScanned = backup.BascScanned == 1;
                    return true;
                }
            }

            return false;
        }

        public void ConcludeCurrentBackup()
        {
            var uncompleted = _schedulesModel.GetUncompletedBackup();
            if (uncompleted != null)
            {
                _schedulesModel.MarkScheduleAsCompleted(uncompleted);
            }
        }

        public bool MarkScheduleAsScanned()
        {
            var schedule = _schedulesModel.GetSchedule(ScheduleId);
            if (schedule == null)
            {
                Logger.Log($"Schedule not found: {ScheduleId}", "SchedulesController");
                return false;
            }
            _schedulesModel.MarkScheduleAsScanned(schedule);
            return true;
        }
    }
}
