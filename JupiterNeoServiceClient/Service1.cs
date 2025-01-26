using AutoMapper;
using Azure;
using JpCommon;
using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Models.Domain;
using JupiterNeoServiceClient.Models.Dto.Backup;
using JupiterNeoServiceClient.Repositories.Interfaces;
using JupiterNeoServiceClient.Utils;
using System.ComponentModel;
using System.Text;
using System.Timers;

namespace JupiterNeoServiceClient
{
    /**
     * 
     * Al comienzo del servicio se debe de verificar.
     * */
    public partial class Service1 : IHostedService
    {
        private System.Timers.Timer? _backupsTimer;
        private System.Timers.Timer? _setupBackupsTimer;

        private BackgroundWorker? uploadWorker;
        private bool isBackupInProgress=false;
        private bool isScanning = false;
        private bool _isCheckingNewBackups = false;

        private string? license;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly JpApi api;
        private readonly IMapper mapper;

        public Service1(IServiceScopeFactory serviceScopeFactory, JpCommon.JpApi api, IMapper mapper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            this.api = api;
            this.mapper = mapper;
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    var dbInit = scope.ServiceProvider.GetRequiredService<DbInit>();
                    dbInit.EnsureTableCreated();
                }
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                this.StopService();
            }

        }

        private void StopService()
        {
            // Logic to stop the service
            Environment.Exit(-1); // Stops the entire application
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.OnServiceStart();
            return Task.CompletedTask;
        }


        public void Dispose()
        {
            // Clean up resources
            _backupsTimer?.Dispose();
            _setupBackupsTimer?.Dispose();
            Console.WriteLine("Service disposed.");
        }


        public void OnServiceStart()
        {
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

                // Tarea repetitiva que se ejecuta cada cierto tiempo.
                this.startBackupTimer();

                /**
                 * Tareas que se realizan una sola vez al día
                 * */

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

        public Task StopAsync(CancellationToken cancellationToken)
        {
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

            return Task.CompletedTask;
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


        public void startBackupTimer()
        {
            _backupsTimer = new System.Timers.Timer();
#if DEBUG
            _backupsTimer.Interval = 1000 * 10; // Pocos segundos para debugging más rápido.
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
                */
                await Task.Run(() => notifyPathsAsync());
                await Task.Run(() => notifyDrivesAsync());
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
                if (this.isBackupInProgress || isScanning || _isCheckingNewBackups)
                {
                    return;
                }
                uploadWorker = new BackgroundWorker();
                uploadWorker.DoWork += BackgroundBatchUploads; // Cuando el timer se termina se corre este método.
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

        bool IsCloudFile(FileInfo fileInfo)
        {
            return fileInfo.FullName.Contains("GoogleDrive") || fileInfo.FullName.EndsWith(".googledrive");
        }

        private bool IsInCommonTemporaryPath(string filePath)
        {
            // Get the system drive dynamically
            string systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";

            // List of common temporary/system paths
            var commonTempPaths = new List<string>
            {
                Path.Combine(systemDrive, "Windows", "Temp"),
                Path.Combine(systemDrive, "Users", "All Users", "Microsoft", "Windows", "WER"),
                Path.Combine(systemDrive, "Users", "Default", "AppData", "Local", "Temp"),
                Path.Combine(systemDrive, "ProgramData", "Microsoft", "Windows", "WER"),
                Path.Combine(systemDrive, "Windows", "WinSxS", "Temp"),
                Path.Combine(systemDrive, "Users", "All Users", "Microsoft"),
                Path.Combine(systemDrive, "Users", "All Users", "NVIDIA"),
                Path.Combine(systemDrive, "Users", "All Users", "NVIDIA Corporation"),
                Path.Combine(systemDrive, "Users", "All Users", "Package Cache"),
                Path.Combine(systemDrive, "Users", "All Users", "GIGABYTE"),
                Path.Combine(systemDrive, "Users", "All Users", "Norton"),
                Path.Combine(systemDrive, "Users", "All Users", "DockerDesktop"),
                Path.Combine(systemDrive, "Users", "All Users", "MySQL"),
                Path.Combine(systemDrive, "Users", "All Users", "Razer"),
                Path.Combine("mingw64", "lib", "python"),
                Path.Combine("mingw64", "share"),
                Path.Combine("biblioteca", "B", "Biblioteca", "vcpkg"),
                Path.Combine("obj", "Debug", "net"),
                Path.Combine("JupiterNeoClientProject-master", "JupiterNeoClientProject-master", "packages"),
                Path.Combine("JupiterNeoClient_v8-main", "ClientConsole"),
                Path.Combine("JupiterNeoClient_v8-main", "JupiterNeoServiceClient"),
                Path.Combine("JupiterNeoClient_v8-main", "JupiterNeoUpdateServ"),
                Path.Combine("vcpkg", "buildtree"),
                Path.Combine("vcpkg", "downloads"),
                Path.Combine("vcpkg", "ports"),
                Path.Combine("vcpkg", "scripts"),
                Path.Combine("vcpkg", "installed"),

                "node_modules",
            };

            // Normalize paths to ensure consistency
            filePath = Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return commonTempPaths.Any(tempPath =>
                filePath.Contains(tempPath, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> ScanPath(string path)
        {

            if (this.license == null)
            {
                Console.WriteLine("No licence was found");
                return false;
            }

            string[] extensions = await this.api.getExtensions(this.license!);
            if (extensions.Length == 0)
            {
                return false;
            }
            bool returnValue = true;
            try
            {
                using(var scope = _serviceScopeFactory.CreateScope())
                {
                    var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

                    if (Directory.Exists(path))
                    {
                        List<string> filesInDir = Helpers.DirSearch(path, extensions);
                        foreach (var file in filesInDir)
                        {
                            // no es un directorio
                            if (!Directory.Exists(file))
                            {

                                FileInfo fileInfo = new FileInfo(file);
                                if (!fileInfo.Exists || (fileInfo.Attributes & FileAttributes.Directory) != 0 || IsCloudFile(fileInfo) || IsInCommonTemporaryPath(file))
                                {
                                    continue;
                                }

                                var fileInDb = await fileRepository.FileByPath(file);
                                if (fileInDb == null)
                                {
                                    var newFile = new Models.Domain.BackupFile
                                    {
                                        Path = file,
                                        CreatedAt = fileInfo.CreationTime.ToString("o"),
                                        UpdatedAt = fileInfo.LastWriteTime.ToString("o"),
                                        AddedAt = DateTime.UtcNow.ToString("o"),
                                        DeletedAt = null,
                                        FailedAttempts = 0,
                                        FileBackedUp = 0,
                                        HistoricFailedAttempts = 0,
                                    };
                                    await fileRepository.InsertFile(newFile);
                                }
                                else
                                {
                                    // El archivo ha cambiado.
                                    if (fileInDb.UpdatedAt != fileInfo.LastWriteTime.ToString("o"))
                                    {
                                        await fileRepository.OnFileModified(file, fileInfo.LastWriteTime.ToString("o"));
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


        private async Task ScanPaths()
        {

            if (this.isScanning)
            {
                return;
            }

            await this.UpdatePathsToBackup();

            using (var scope = _serviceScopeFactory.CreateScope())
            {

                var backupPathRepo = scope.ServiceProvider.GetRequiredService<IBackupPathRepository>();
                var backupRepository = scope.ServiceProvider.GetRequiredService<IBackupRepository>();
                var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();

                // The software is ready to backup. However we haven't scanned the system for files for this schedule.
                var paths = await backupPathRepo.GetAllPaths();

                if (paths.Count > 0)
                {
                    this.isScanning = true;
                    try
                    {
                        /***
                         * Escanear todos los paths registrados (obtenidos desde la API), por ejemplo: C:\\Users\\Documents, C:\\Users\\Downloads, D:\\, etc
                         */
                        foreach (var row in paths)
                        {
                            if (row.Path != null)
                            {
                                await this.ScanPath(row.Path);
                            }
                        }

                        /***
                         * En este punto ya todas las rutas han sido escaneadas y podemos marcar localmente que el escaneo finalizó y reportarle a la API la cantidad de elementos escaneados.
                         */
                        var currentBackup = await backupRepository.GetUncompletedBackup();
                        if (currentBackup == null)
                        {
                            throw new Exception("Se ha escaneado pero la base de datos no tiene un backup en progreso.");
                        }

                        var expectedFilesToUpload = await fileRepository.PendingFilesCount(); // Cantidad de archivos no respaldados que hemos escaneado. Representan la cantidad de archivos que se subirán al servidor.

                        /***
                         * En caso de que se haya escaneado pero no se detecto ni un solo archivo podemos marcar el backup como escaneado y completado.
                         * En caso contrario que si hayamos encontrado archivos que subir podemos solo marcar el backup como escaneado.
                         */

                        if (expectedFilesToUpload > 0)
                        {
                            var response = await this.api.UpdateScannedFilesForBackup(this.license!, currentBackup.BackupId, expectedFilesToUpload); // .BackupId es el ID de la nube, .Id es el ID local
                            response.EnsureSuccessStatusCode();
                            // Si la api nos responde con éxito la actualización de elementos escaneados podemos proceder a marcar el backup como escaneado localmente.
                            await backupRepository.MarkBackupAsScanned(currentBackup.Id);
                        }else if (currentBackup.IsStarted == 1)
                        {
                            // Solo terminar el backup si no hay más archivos que subir y además no hay archivos que subir.
                            var response = await this.api.UpdateScannedFilesForBackup(this.license!, currentBackup.BackupId, expectedFilesToUpload); // .BackupId es el ID de la nube, .Id es el ID local
                            response.EnsureSuccessStatusCode();
                            await backupRepository.MarkBackupAsScanned(currentBackup.Id);
                            await backupRepository.MarkBackupAsCompleted(currentBackup.Id);
                        }else
                        {
                            // No hay nuevos archivos escaneados (0), podemos marcar como escaneado.
                            var response = await this.api.UpdateScannedFilesForBackup(this.license!, currentBackup.BackupId, expectedFilesToUpload);
                            response.EnsureSuccessStatusCode();
                            await backupRepository.MarkBackupAsScanned(currentBackup.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to scan " + ex.Message);
                    }
                    finally
                    {
                        this.isScanning = false;
                    }

                }
            }


        }

        public string? GetDeletedFolder(string path)
        {
            string[] folders = path.Split('\\'); // Split the path by folder separator
            string currentPath = string.Empty;

            int folderCounter = 0;

            foreach (string folder in folders)
            {
                var append = folderCounter == folders.Length - 1 ? "" : "\\";
                folderCounter++;

                currentPath = Path.Combine(currentPath, folder + append); // Build the current path

                if (File.Exists(currentPath))
                {
                    return null;
                }
                // Check if the folder exists, if not return the previous path
                if (!Directory.Exists(currentPath))
                {
                    return Path.GetDirectoryName(currentPath)!.Replace("\\", @"\");
                }
            }
            return null;
        }



        /**
         *   Este método se ejecuta una vez al día para actualizar las rutas de respaldo.
         * */
        private async Task UpdatePathsToBackup()
        {
            using(var scope = _serviceScopeFactory.CreateScope())
            {
                var backupPathsRepository = scope.ServiceProvider.GetRequiredService<IBackupPathRepository>();
                var result = await api.getSchedulesAsync(this.license!);
                if (result.paths != null)
                {
                    foreach (var path in result.paths)
                    {
                        var pathInLocalDb = await backupPathsRepository.ByPath(path);
                        if (pathInLocalDb == null)
                        {
                            await backupPathsRepository.AddPath(path);
                        }
                    }
                }

            }
        }


        /***
         * #1 - Punto más importante para los backups.
         * Este método se ejecuta de forma periodica, aquí se hace lo siguiente:
         * 1. Obtener ID de backup si no se tiene o detener la ejecucución si hoy ya se realizó el backup.
         * 2. Al obtener un ID nuevo del cual no se ha escaneado se inicia escaneo
         * 3. Cuando ya se realizó un escaneo correspondiente para este ID de backup se procede a hacer los backups.
         * */
        private async void BackgroundBatchUploads(object? sender, DoWorkEventArgs e)
        {

            // No continuar si el escaneo de rutas está en progreso o si ya hay un backup en progreso. (Subida de archivos)
            if (isScanning || isBackupInProgress || _isCheckingNewBackups) {
                return;
            }
            
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {

                    this._isCheckingNewBackups = true;

                    var metadataRepository = scope.ServiceProvider.GetRequiredService<ImetadataRepository>();
                    // Revisar la base de datos localmente para ver si hay un backup registrado.
                    var backupRepository = scope.ServiceProvider.GetRequiredService<IBackupRepository>();
                    var filesRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
                    this.license = await metadataRepository.GetLicense();


                    var locallyUncompletedBackup = await backupRepository.GetUncompletedBackup();

                    if (license == null)
                    {
                        Console.WriteLine("No hay una licencia registrada");
                        this._isCheckingNewBackups = false;
                        return;
                    }

                    if (locallyUncompletedBackup == null)
                    {

                        /***
                         * En este punto no hay un backup en progreso en la base de datos local.
                         * Se procede a consultar la API para ver si nos devuelve un ID de backup.
                         * Nos devolvera un ID si ya hay uno en progreso registrado en la API o si hoy no se ha realizado un backup.
                         * En caso de que la API ya tenga un backup completado de hoy, no se devolvera un ID, más bien obtendremos NULL.
                         * 
                         */
                        var backupIdResult = await this.api.GetBackupIdV2Async(license);
                        if (backupIdResult == null)
                        {
                            this._isCheckingNewBackups = false;
                            Console.WriteLine("No se pudo obtener un backupId");
                            return;
                        }


                        var backupByIdLocal = await backupRepository.GetBackupByBackupId(backupIdResult.Value); // Obtener por ID de backup de la nube y comparar con el local.
                        if (backupByIdLocal == null)
                        {
                            // La API nos ha devuelto un ID, hay que registrarlo en la base de datos local (Ya también verificamos que no existe localmente) y escanear archivos para realizar este backup.
                            var backup = new AddBackupDto
                            {
                                BackupId = backupIdResult.Value,
                                CreatedAt = DateTime.UtcNow.ToString("o"),
                                IsCompleted = 0,
                                IsScanned = 0,
                                IsStarted = 0,
                            };
                            await backupRepository.AddBackup(mapper.Map<Backup>(backup));
                            await this.ScanPaths();
                        }
                        this._isCheckingNewBackups = false;
                        return;
                    }

                    this._isCheckingNewBackups = false;

                    // Si el backup ya se termino de subir, no hacer nada.
                    if (locallyUncompletedBackup.IsCompleted == 1)
                    {
                        // Conclude backup in case it hasnt been concluded.
                        var response = await this.api.concludeBackup(this.license, locallyUncompletedBackup.BackupId.ToString());
                        response.EnsureSuccessStatusCode();
                        await backupRepository.MarkBackupAsCompleted(locallyUncompletedBackup.Id);
                        this.isBackupInProgress = false;
                        return;
                    }



                    // El backup que está incompleto no es nulo.
                    // Si no está escaneado procedemos a escanear.
                    if (locallyUncompletedBackup.IsScanned == 0)
                    {
                        // Si el backup no ha sido escaneado, se procede a escanear.
                        await this.ScanPaths();
                        return;
                    }

                    if (locallyUncompletedBackup.IsStarted == 0)
                    {

                        /***
                         * Cuando se comienza un backup nuevo (IsStarted = 0) y si es que hubo un backup anterior hay que hacer lo siguiente:
                         * 1. Reiniciar el contador de los archivos fallidos. (Se reinicia el contador para que los archivos que no se pudieron subir en el backup anterior, no se suban de nuevo)
                         * 2. Si se hizo un backup/escaneo anterior en el cual se subieron archivos, se debe verificar si hay archivos borrados y en tal caso reportarlos como borrados a la API.
                         * 
                         * */
                        // Marcamos el backup como comenzado en la base de datos local.
                        await backupRepository.MarkBackupAsStarted(locallyUncompletedBackup.Id);
                        // Reiniciar archivos fallidos
                        await filesRepository.ResetFailed();

                        // Verificar si hay archivos borrados
                        var allFiles = await filesRepository.ListBackedUp();
                        foreach (var f in allFiles)
                        {
                            if (!File.Exists(f.Path) && f.Path != null)
                            {
                                try
                                {
                                var response = await this.api.reportDeletedFile(this.license, locallyUncompletedBackup.BackupId.ToString(), f.Path);
                                response.EnsureSuccessStatusCode();
                                await filesRepository.MarkAsDeleted(f.Path);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("ex: "+ ex.Message);
                                }
                            }
                        }


                    }

                    var canConnectToServer = await this.api.isServerResponseOk();
                    if (!canConnectToServer)
                    {
                        return;
                    }

    
                    // En este punto hay un backup sin completar, y tenemos conexión con la API, podemos comenzar a subir por lotes.
                    // Subir archivos por lotes

                    this.isBackupInProgress = true;

                    var filesToUpload = await filesRepository.GetBackedUpNull(30); // Obtener 30 archivos que no se han respaldado.


                    if (filesToUpload.Count() > 0)
                    {

                        foreach (var file in filesToUpload)
                        {

                            if (file == null)
                            {
                                continue;
                            }
                            /*** Upload File Flow **/
                            try
                            {
                                string readFrom = file.Path!;

                                if (!File.Exists(file.Path))
                                {

                                    if (file.FileBackedUp == 1)
                                    {
                                        // report deleted only when the file has previously been backed up.
                                        try
                                        {
                                            var response = await this.api.reportDeletedFile(this.license, locallyUncompletedBackup.BackupId.ToString(), file.Path!);
                                            response.EnsureSuccessStatusCode();
                                            await filesRepository.MarkAsDeleted(file.Path!);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Log(ex, "---vvvv-A--");
                                        }
                                    }

                                    // Detect if a deleted subfolder is the cause of the deletion of the file.
                                    // In that case more than a file could have been deleted (could be thousands of files) and if they haven't been uploaded we can simply remove them.
                                    var deletedPath = GetDeletedFolder(file.Path!);
                                    if (deletedPath != null)
                                    {
                                        // A folder was deleted. Delete all files in the database that belonged to that folder and that haven't been backed up.
                                        await filesRepository.DeleteStartsWithPathAndHasBackedUp(deletedPath);
                                    }
                                }

                                if (FileProcessor.FileCanBeRead(file.Path!))
                                {
                                    try
                                    {
                                        var response = await this.api.uploadFile(this.license, locallyUncompletedBackup.BackupId.ToString(), file.Path!, readFrom);
                                        response.EnsureSuccessStatusCode();
                                        await filesRepository.MarkAsBackedUp(file.Id);
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Log(ex, "--vvvv--001--");
                                        await filesRepository.MarkAsFailed(file.Id);
                                    }
                                }
                                else
                                {
                                    // NOTE: Here we would previously try to create a shadow copy (VSS) but it would create some issues in the user's machine. That code got removed.
                                    await filesRepository.MarkAsFailed(file.Id);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex, "---vvvv-03--");
                            }
                        }

                        this.isBackupInProgress = false;
                    }
                    else
                    {

                        var response = await this.api.concludeBackup(this.license, locallyUncompletedBackup.BackupId.ToString());
                        response.EnsureSuccessStatusCode();
                        await backupRepository.MarkBackupAsCompleted(locallyUncompletedBackup.Id);

                        this.isBackupInProgress = false;

                    }
                    return;
                }
            } catch(Exception ex)
            {
                Console.WriteLine("ex: " + ex.Message + ex.InnerException);
                this.isBackupInProgress = false;
                this._isCheckingNewBackups = false;
                this.isScanning = false;
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
        }

    }
}
