using JpCommon;
using JupiterNeoServiceClient.Controller;
using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using System.ComponentModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Timers;

namespace JupiterNeoServiceClient
{
    /**
     * 
     * Al comienzo del servicio se debe de verificar.
     * */
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer? _schedulesTimer;
        private System.Timers.Timer? _backupsTimer;
        private System.Timers.Timer? _setupBackupsTimer;

        private BackgroundWorker? uploadWorker;
        private bool isBackupInProgress { get; set; }
        private bool isVerifying { get; set; }
        private bool hasStartedNewBackup { get; set; }

        private bool wereSchedulesChecked = false;
        private bool isScanning = false;

        private bool isDatabaseUpToDate = false;

        private DatabaseManager dbManager = new DatabaseManager();
        private bool isUpdatingDatabase = false;

        public Service1()
        {
            InitializeComponent();

            // Setup Service
            this.ServiceName = "JupiterNeoClient";
            this.CanStop = true;
            this.CanPauseAndContinue = true;

            // Setup logging
            this.AutoLog = false;

            try
            {
                if (this.EventLog != null)
                {
                    ((ISupportInitialize)this.EventLog).BeginInit();

                    if (!EventLog.SourceExists(this.ServiceName))
                    {
                        EventLog.CreateEventSource(this.ServiceName, "Application");
                    }

                    ((ISupportInitialize)this.EventLog).EndInit();
                    this.EventLog.Source = this.ServiceName;
                    this.EventLog.Log = "Application";
                }
                else
                {
                    Console.WriteLine("EventLog is null. Logging is not configured.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up EventLog: {ex.Message}");
            }

            // Initialize values
            this.isBackupInProgress = false;
            this.isVerifying = false;
        }


   
        protected override void OnStart(string[] args)
        {
            this.OnServiceStart();
        }

        public void OnServiceStart()
        {
            this.EventLog.WriteEntry("OnServiceStart");
            try
            {
                try
                {
                    this.startSetupBackupRequirementsTimer();
                    JpDeviceInfo.GetDeviceInfo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                this.startScheduleTimer();
                this.startBackupTimer();
                new RunOnceADay(this.notifyProgramsInstalled);
                new RunOnceADay(this.notifyVersionAsync);
                new RunOnceADay(this.notifyPathsAsync);
                new RunOnceADay(this.notifyDrivesAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                writeExceptionLog(ex, "OnServiceStart/Catch");
            }
        }

        protected override void OnStop()
        {
            try
            {
                if (_schedulesTimer != null) {
                    _schedulesTimer.Stop();
                    _schedulesTimer.Dispose();
                }
            }
            catch (Exception ex)
            {
                writeExceptionLog(ex, "OnStop/Catch - schedulesTimer");
                Console.WriteLine(ex.Message);
            }

            try
            {
                if (_backupsTimer != null)
                {
                    _backupsTimer.Stop();
                    _backupsTimer.Dispose();
                }
            }
            catch (Exception ex)
            {
                writeExceptionLog(ex, "OnStop/Catch - _backupsTimer");
                Console.WriteLine(ex.Message);
            }

            try
            {
                // Detener todos los timers
                try
                {
                    if (uploadWorker != null) {
                        if (uploadWorker.IsBusy)
                        {
                            // Cancelar el trabajo en el background.
                            uploadWorker.CancelAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    writeExceptionLog(ex, "[isBusy]");
                }

                if (BaseModel.connection != null)
                {
                    // Intentar cerrar la conexión SQLite
                    BaseModel.connection.Close();
                }
                // Indicar al garbage collector que limpie
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                writeExceptionLog(ex, "OnStop/Catch");
            }
        }

        private async Task CheckSchedulesAsync()
        {
            try
            {
                await this.verifySchedules();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
                this.EventLog.WriteEntry("[SQLite-Sch/Catch] " + ex.Message);
            }
            catch (Exception ex)
            {
                this.EventLog.WriteEntry("[CheckSchedules/Catch] " + ex.Message);
            }
        }

        public async Task verifySchedules()
        {
            if (this.isVerifying)
            {
                return;
            }
            this.isVerifying = true;
            try
            {
                var schedulesController = new SchedulesController();
                await schedulesController.verifySchedules();
                this.isVerifying = false;
                this.wereSchedulesChecked = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                writeExceptionLog(ex, "verifySc/Catch");
                this.isVerifying = false;
            }
        }

        private async Task<bool> notifyPathsAsync()
        {
            bool didItNotifyCorrecty;
            try
            {
                var api = new JpApi();
                var controller = new FileController();
                var result = await api.notifyPathsAvailable(controller.license);
                result.EnsureSuccessStatusCode();
                didItNotifyCorrecty = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                didItNotifyCorrecty = false;
            }
            return didItNotifyCorrecty;
        }

        private async Task<bool> notifyDrivesAsync()
        {
            bool didNotifyCorrectly;
            try
            {
                var api = new JpApi();
                var controller = new FileController();
                List<string> drivesFound = JpPaths.GetAllFixedDrives();
                var result = await api.notifyDisksAvailable(controller.license, drivesFound);
                result.EnsureSuccessStatusCode();
                didNotifyCorrectly = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                didNotifyCorrectly = false;
            }
            return didNotifyCorrectly;
        }

        private async Task<bool> notifyVersionAsync()
        {
            bool notifiedSuccessfully = false;
            try
            {
                var filesController = new FileController();
                var fileManager = new FileManager();

                if (filesController.license.Length > 0)
                {
                    var crs = await filesController.api.isServerResponseOk();
                    if (!crs)
                    {
                        return false;
                    }
                    var version = fileManager.getCurrentVersion();
                    HttpResponseMessage response = await filesController.api.notifyVersion(filesController.license, version);
                    response.EnsureSuccessStatusCode();
                    notifiedSuccessfully = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                writeExceptionLog(ex, "notifyVersion");
            }
            return notifiedSuccessfully;
        }

        /**
         * 
         * Revisar si hay un nuevo horario establecido.
         * 
         * */
        public void startScheduleTimer()
        {
            _schedulesTimer = new System.Timers.Timer();
#if DEBUG
            _schedulesTimer.Interval = 1000 * 10; // Each second
#else
            _schedulesTimer.Interval = 1000 * 60 * 10; // 10 minutes
#endif
            _schedulesTimer.Elapsed += async (sender, e) => await this.CheckSchedulesAsync();
            _schedulesTimer.Start();
        }

        public void startBackupTimer()
        {
            _backupsTimer = new System.Timers.Timer();
#if DEBUG
            _backupsTimer.Interval = 1000 * 30;
#else
            _backupsTimer.Interval = 1000 * 60 * 2; // Cada 2 minutos
#endif
            _backupsTimer.Elapsed += BackupCheckTimedOut;
            _backupsTimer.Start();
        }

        public void startSetupBackupRequirementsTimer()
        {
            _setupBackupsTimer = new System.Timers.Timer();
#if DEBUG
            _setupBackupsTimer.Interval = 1000 * 30;
#else
            _setupBackupsTimer.Interval = 1000 * 60 * 3;
#endif
            _setupBackupsTimer.Elapsed += async (sender, e) => await this.setupBackupRequirements();
            _setupBackupsTimer.Start();
        }

        public async Task setupBackupRequirements()
        {
            try
            {
                /**
                 * Try to run this tasks sequentially to try to have a backup ready asap.
                 * 
                 */
                await Task.Run(() => notifyPathsAsync());
                await Task.Run(() => notifyDrivesAsync());
                await Task.Run(() => CheckSchedulesAsync());
                Console.WriteLine("Ready to backup.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("something failed: " + ex.Message);
            }
        }

        public void BackupCheckTimedOut(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.isBackupInProgress || !this.wereSchedulesChecked || isScanning)
                {
                    return;
                }
                uploadWorker = new BackgroundWorker();
                uploadWorker.DoWork += BackgroundBatchUploads;
                uploadWorker.WorkerReportsProgress = true;
                // Start the background worker
                uploadWorker.RunWorkerAsync();
                uploadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(batchUploadWorkerCompleted);
            }
            catch (Exception ex)
            {
                writeExceptionLog(ex, "bbCheckTimedOut");

            }
        }
        private void batchUploadWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            this.isBackupInProgress = false;
        }



        private async Task<bool> notifyProgramsInstalled()
        {

            bool wasSuccessful;
            try
            {
                var controller = new ProgramsController();
                wasSuccessful = await controller.notifyPrograms();
            }
            catch (Exception ex)
            {
                writeExceptionLog(ex, "notifyProgramsInstalled/catch");
                wasSuccessful = false;
            }
            return wasSuccessful;
        }
        private async void BackgroundBatchUploads(object? sender, DoWorkEventArgs e)
        {
            if (this.isBackupInProgress)
            {
                return;
            }
            try
            {
                MetaDataController metaDataController = new MetaDataController();
                SchedulesController schedulesController = new SchedulesController();
                FileController fileController = new FileController();
                var crs = await metaDataController.api.isServerResponseOk();
                if (!crs)
                {
                    return;
                }
                bool shouldBackup = schedulesController.shouldBackup();
                string? backupId = metaDataController.getBackupId();
                string s = shouldBackup ? "Yes" : "No";
                bool shouldStop = false;
                if (shouldBackup && !schedulesController.WasSystemScanned && !isScanning)
                {
                    // The software is ready to backup. However we haven't scanned the system for files for this schedule.
                    BackupPathModel backupPathModel = new BackupPathModel();
                    var paths = backupPathModel.getAllPaths();
                    if (paths.Count > 0)
                    {
                        this.isScanning = true;
                        bool wasUpdated = false;
                        try
                        {
                            foreach (var row in paths)
                            {
                                if (row.bapa_path != null)
                                {
                                    await schedulesController.ScanPath(row.bapa_path);
                                }
                            }
                            wasUpdated = schedulesController.markScheduleAsScanned();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Failed to scan " + ex.Message);
                        }
                        finally
                        {
                            this.isScanning = false;
                        }
                        if (!wasUpdated)
                        {
                            shouldStop = true;
                        }
                    }
                    else
                    {
                        shouldStop = true;
                    }
                }
                if (shouldStop || isScanning)
                {
                    return;
                }
                int pendingFilesCount = fileController.getPendingFilesCount();
                if (shouldBackup && backupId == null)
                {
                    /**
                     * Si debe de hacer un backup y el backupId es es nulo quiere decir que no tenemos un id al cual subir archivos.
                     * Esto solo se ejecuta la primera vez que inicial el backup.
                     * */
                    await metaDataController.requestBackup(pendingFilesCount);
                    backupId = metaDataController.getBackupId();
                    if (backupId == null)
                    {
                        this.EventLog.WriteEntry("[bcc] bcID Is Null");
                    }
                    else
                    {
                        this.hasStartedNewBackup = true;
                    }
                }
                if (this.hasStartedNewBackup)
                {
                    try
                    {
                        fileController.resetFailed();
                        await fileController.reportDeletedFiles(backupId);
                        this.hasStartedNewBackup = false;
                    }
                    catch (Exception ex)
                    {
                        writeExceptionLog(ex, "hast-started-new-failed/");
                    }
                }
                if (backupId != null)
                {
                    this.isBackupInProgress = true;
                    var batch = fileController.getPendingUploadsBatch();
                    var totalCount = fileController.totalFilesInDb();
                    if (batch.Count() > 0)
                    {
                        if (shouldBackup)
                        {
                            try
                            {
                                await fileController.uploadBatch(backupId);
                            }
                            catch (Exception ex)
                            {

                                writeExceptionLog(ex, "uBatch");
                                this.isBackupInProgress = false;
                            }
                        }
                        this.isBackupInProgress = false;
                    }
                    else /*if (totalCount > 0)*/
                    {
                        // conclude backup
                        //  solo se puede concluir el backup si hay archivos en la base de datos.
                        try
                        {
                            var bid = metaDataController.getBackupId();

                            if (bid != null)
                            {
                                await metaDataController.concludeBackup(bid);
                                schedulesController.concludeCurrentBackup();
                            }
                        }
                        catch (Exception ex)
                        {
                            writeExceptionLog(ex, "BackgroundBatchUploads / Catch");
                        }
                        this.isBackupInProgress = false;
                    }
                }
                else
                {
                    this.isBackupInProgress = false;
                }
            }
            catch (Exception ex)
            {
                writeExceptionLog(ex, "bcc -ex-");
                this.isBackupInProgress = false;
            }
        }

        private void writeExceptionLog(Exception ex, string level)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine($"Level: [{level}]");
            messageBuilder.AppendLine("An exception occurred:");
            messageBuilder.AppendLine("Message: " + ex.Message);
            messageBuilder.AppendLine("Stack Trace:");
            messageBuilder.AppendLine(ex.StackTrace);
            // Log the exception message and stack trace
            this.EventLog.WriteEntry("bcc -ex- " + messageBuilder.ToString());
        }

    }
}
