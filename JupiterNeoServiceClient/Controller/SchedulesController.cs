using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Controller
{
    public class SchedulesController : BaseController
    {

        public SchedulesModel sm = new SchedulesModel();

        public FileModel fileModel = new FileModel();
        public string scheduleId { get; set; }

        public DateTime lastTimeScanned { get; set; }

        public bool WasSystemScanned { get; set; }

        public SchedulesController()
        {
            this.sm = new SchedulesModel();
            this.fileModel = new FileModel();
            this.lastTimeScanned = DateTime.MinValue;
        }

        public async Task verifySchedules()
        {
            if (this.License != null && this.License.Length > 0)
            {
                var result = await Api.getSchedulesAsync(this.license);
                if (result.schedules != null)
                {
                    foreach (var schedule in result.schedules)
                    {
                        var sc = sm.getSchedule(schedule);
                        if (sc == null)
                        {
                            sm.insertSchedule(schedule);
                        }
                    }
                }
                if (result.paths != null)
                {
                    BackupPathController backupPathCtrl = new BackupPathController();
                    backupPathCtrl.updatePaths(result.paths);
                }
                result = null;
            }
        }

        public async Task<bool> ScanPath(string path)
        {
            string[] extensions = await this.api.getExtensions(this.license);
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
                        // no es un directorio
                        if (!Directory.Exists(file))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            // El archivo existe en el sistema y no en la base de datos.
                            if (fileInfo.Exists)
                            {
                                var fileInDb = fileModel.fileByPath(file);
                                string lastWrite = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                                if (fileInDb == null)
                                {
                                    var ins = fileModel.insertFile(file, fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"), lastWrite);
                                }
                                else
                                {
                                    // El archivo ha cambiado.
                                    if (fileInDb.file_updated_at != lastWrite)
                                    {
                                        fileModel.onFileModified(file, lastWrite);
                                    }
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


        public bool shouldBackup()
        {
            bool returnValue = false;

            var uncompleted = this.sm.getUncompletedBackup();

            if (uncompleted != null)
            {
                this.scheduleId = uncompleted.basc_id;
                this.WasSystemScanned = uncompleted.basc_scanned == 1;

                returnValue = true;
            }
            else
            {
                var unstarted = this.sm.getAllUnstartedBackups();
                if (unstarted.Count() > 0)
                {
                    foreach (var b in unstarted)
                    {
                        if (Helpers.HasTimeElapsed(b.basc_time))
                        {
                            this.scheduleId = b.basc_id;
                            this.sm.markScheduleAsStarted(b);
                            this.WasSystemScanned = b.basc_scanned == 1;
                            returnValue = true;
                            break;
                        }
                    }
                }

            }

            if (!this.WasSystemScanned)
            {
            }

            return returnValue;
        }

        public void concludeCurrentBackup()
        {
            var uncompleted = this.sm.getUncompletedBackup();
            if (uncompleted != null)
            {
                this.sm.markScheduleAsCompleted(uncompleted);
            }
        }

        public bool markScheduleAsScanned()
        {
            SModel schedule = this.sm.getSchedue(this.scheduleId);
            if (schedule == null)
            {
                return false;
            }
            this.sm.markScheduleAsScanned(schedule);
            return true;
        }
    }
}
