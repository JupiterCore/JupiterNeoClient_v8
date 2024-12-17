using JpCommon;
using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Controllers.JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;


namespace JupiterNeoServiceClient.Controller
{
    public class SchedulesController : BaseController
    {
        public SchedulesModel sm = new();
        public FileModel fileModel = new();
        public string scheduleId { get; set; }
        public DateTime lastTimeScanned { get; set; } = DateTime.MinValue;
        public bool WasSystemScanned { get; set; }

        // Constructor ajustado para recibir y pasar parámetros a BaseController
        public SchedulesController(MetadataModel metaModel, JpApi api)
            : base(metaModel, api)
        {
            sm = new SchedulesModel();
            fileModel = new FileModel();
        }

        public async Task VerifySchedules()
        {
            if (!string.IsNullOrEmpty(License))
            {
                var result = await Api.GetSchedulesAsync(License);
                if (result?.Schedules != null)
                {
                    foreach (var schedule in result.Schedules)
                    {
                        var sc = sm.GetSchedule(schedule);
                        if (sc == null)
                        {
                            sm.InsertSchedule(schedule);
                        }
                    }
                }
                if (result?.Paths != null)
                {
                    BackupPathModel backupPathModel = new BackupPathModel();
                    BackupPathController backupPathCtrl = new BackupPathController(backupPathModel);
                    backupPathCtrl.UpdatePaths(result.Paths);
                }
            }
        }

        public async Task<bool> ScanPath(string path)
        {
            string[] extensions = await Api.GetExtensionsAsync(License);
            if (extensions.Length == 0)
            {
                return false;
            }

            bool returnValue = true;
            try
            {
                if (Directory.Exists(path))
                {
                    List<string> filesInDir = Helpers.DirSearch(path, extensions);
                    foreach (var file in filesInDir)
                    {
                        if (!Directory.Exists(file)) // No es un directorio
                        {
                            FileInfo fileInfo = new(file);
                            if (fileInfo.Exists) // El archivo existe en el sistema
                            {
                                var fileInDb = fileModel.FileByPath(file);
                                string lastWrite = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                                if (fileInDb == null)
                                {
                                    fileModel.InsertFile(file, fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), lastWrite);
                                }
                                else if (fileInDb.file_updated_at != lastWrite) // El archivo ha cambiado
                                {
                                    fileModel.OnFileModified(file, lastWrite);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "---P50B3000--");
                returnValue = false;
            }
            return returnValue;
        }

        public bool ShouldBackup()
        {
            bool returnValue = false;

            var uncompleted = sm.GetUncompletedBackup();

            if (uncompleted != null)
            {
                scheduleId = uncompleted.basc_id;
                WasSystemScanned = uncompleted.basc_scanned == 1;
                returnValue = true;
            }
            else
            {
                var unstarted = sm.GetAllUnstartedBackups();
                if (unstarted.Any())
                {
                    foreach (var b in unstarted)
                    {
                        if (Helpers.HasTimeElapsed(b.basc_time))
                        {
                            scheduleId = b.basc_id;
                            sm.MarkScheduleAsStarted(b);
                            WasSystemScanned = b.basc_scanned == 1;
                            returnValue = true;
                            break;
                        }
                    }
                }
            }

            return returnValue;
        }

        public void ConcludeCurrentBackup()
        {
            var uncompleted = sm.GetUncompletedBackup();
            if (uncompleted != null)
            {
                sm.MarkScheduleAsCompleted(uncompleted);
            }
        }

        public bool MarkScheduleAsScanned()
        {
            var schedule = sm.GetSchedule(scheduleId);
            if (schedule == null)
            {
                return false;
            }
            sm.MarkScheduleAsScanned(schedule);
            return true;
        }
    }
}