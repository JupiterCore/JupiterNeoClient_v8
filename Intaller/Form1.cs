using CommonClasessLibrary;
using JpCommon;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.IO.Compression;
using System.Reflection;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace Installer
{
    public partial class Form1 : Form
    {
        bool isInstalling = false;
        Downloader downloader = new Downloader();
        FilesManager filesManager = new FilesManager();

        public bool shouldRequestLicense { get; set; }
        private bool _isFullRemove { get; set; }

        public Form1()
        {
            InitializeComponent();
            this.Text = "Instalador - Jupiter NEO";
            this.btnFullRemove.Visible = false;
            this.KeyPreview = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (this.isInstalling)
            {
                return;
            }
            this.installBtn.Enabled = false;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += RunInstallProcess!;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted!;
            worker.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void RunInstallProcess(object sender, DoWorkEventArgs e)
        {
            try
            {
                string updaterFolder = Path.Combine(filesManager.diskPath!, JpConstants.UpdaterFolderName);
                string serviceExe = Path.Combine(filesManager.installationPath!, JpConstants.ClientExeName);
                string updaterExe = Path.Combine(updaterFolder, JpConstants.UpdaterExeName);
                JpServiceManager serviceManager = new JpServiceManager();
                if (!Directory.Exists(filesManager.installationPath!))
                {
                    Directory.CreateDirectory(filesManager.installationPath!);
                }
                if (!Directory.Exists(updaterFolder))
                {
                    Directory.CreateDirectory(updaterFolder);
                }
                if (serviceManager.IsServiceInstalled(JpConstants.ClientServiceName))
                {
                    serviceManager.UninstallService(JpConstants.ClientServiceName);
                }
                if (serviceManager.IsServiceInstalled(JpConstants.UpdaterServiceName))
                {
                    serviceManager.UninstallService(JpConstants.ClientServiceName);
                }
                this.isInstalling = true;
                labelInfo.Invoke((MethodInvoker)(() =>
                {
                    labelInfo.Text = "Preparando...";
                }));
                progressBar.Invoke((MethodInvoker)(() =>
                {
                    progressBar.Visible = true;
                }));
                labelInfo.Invoke((MethodInvoker)(() =>
                {
                    labelInfo.Text = "Obteniendo instalador";
                }));
                string zipProgramName = "JupiterNeoClient.zip";
                string updaterZipName = "updater-1.0.0.14.zip";
                string finalZipPath = Path.Combine(filesManager.diskPath!, zipProgramName);
                string finalUpdaterPath = Path.Combine(filesManager.diskPath!, updaterZipName);
                string tmpPath = JpPaths.CreateTemporaryPath();
                var assembly = Assembly.GetExecutingAssembly();
                string tmpZipPath = Path.Combine(tmpPath, zipProgramName);
                string tmpSqlitePath = Path.Combine(tmpPath, JpConstants.SQLiteNameNeo);
                string tmpUpdaterPath = Path.Combine(tmpPath, updaterZipName);
                JpResourceExtractor.ExtractEmbeddedResource(assembly, updaterZipName, tmpPath);
                JpResourceExtractor.ExtractEmbeddedResource(assembly, zipProgramName, tmpPath);
                JpResourceExtractor.ExtractEmbeddedResource(assembly, JpConstants.SQLiteNameNeo, tmpPath);
                /**
                 * Si el servicio ya est  instalado hay que detenerlo y luego desinstalarlo para evitar que
                 * el folder de instalaci n este bloqueado y no se pueda continuar.
                 * */
                if (File.Exists(tmpZipPath) && File.Exists(tmpSqlitePath))
                {
                    try
                    {
                        this.labelInfo.Invoke((MethodInvoker)(() =>
                        {
                            this.labelInfo.Text = "Desinstalando servicio actual";
                        }));
                        string finalInstallationPath = Path.Combine(filesManager.diskPath!, JpConstants.ClientFolderName);
                        string exePath = Path.Combine(finalInstallationPath, JpConstants.ClientExeName);
                        this.labelInfo.Invoke((MethodInvoker)(() =>
                        {
                            this.labelInfo.Text = "Desinstalacion completa.";
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.labelInfo.Invoke((MethodInvoker)(() =>
                        {
                            this.labelInfo.Text = ex.Message;
                        }));
                    }
                }
                else
                {
                    this.labelInfo.Invoke((MethodInvoker)(() =>
                    {
                        this.labelInfo.Text = "Fallo al extraer los elementos embebidos en una locaci n temporal.";
                    }));
                    return;
                }

                // copiar los servicios de cliente y actualizador en AppData\Local para su posterior extracci n.
                File.Copy(tmpZipPath, finalZipPath, true);
                File.Copy(tmpUpdaterPath, finalUpdaterPath, true);

                string finalApplicationPath = Path.Combine(filesManager.diskPath!, JpConstants.ClientFolderName);
                string finalDbLocation = Path.Combine(finalApplicationPath, JpConstants.SQLiteNameNeo);
                string srcSqlite = Path.Combine(tmpPath, JpConstants.SQLiteNameNeo);

                this.progressBar.Invoke((MethodInvoker)(() =>
                {
                    this.progressBar.Style = ProgressBarStyle.Continuous;
                    this.progressBar.Visible = true;
                    this.progressBar.Maximum = 100;

                }));

                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = "Extrayendo archivos.";
                }));

                // Anteriormente: // ZipFile.ExtractToDirectory(finalZipPath, filesManager.diskPath!, true);
                /*
                 * Extraer cada archivo del zip y colocarlo en la carpeta de instalaci n.
                 */
                using (ZipArchive archive = ZipFile.Open(finalZipPath, ZipArchiveMode.Read))
                {

                    var entries = archive.Entries;
                    double percentage = 0;
                    int counter = 0;
                    foreach (ZipArchiveEntry entry in entries)
                    {
                        percentage = (counter * 100) / entries.Count();
                        this.labelInfo.Invoke((MethodInvoker)(() =>
                        {
                            this.labelInfo.Text = $"{percentage}%";
                        }));
                        this.progressBar.Invoke((MethodInvoker)(() =>
                        {
                            this.progressBar.Step = (int)percentage;
                            this.progressBar.Value = (int)percentage;
                        }));
                        Thread.Sleep(5);
                        // Determine the target file path
                        string containerFilePath = Path.Combine(filesManager.diskPath!, JpConstants.ClientFolderName);
                        string targetFilePath = Path.Combine(containerFilePath, entry.Name);
                        // Create the directory if it doesn't exist
                        // Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                        if (targetFilePath != containerFilePath)
                        {
                            // Extract the entry, overwriting if the file already exists
                            entry.ExtractToFile(targetFilePath, true);
                        }
                        counter++;
                    }
                }

                this.progressBar.Invoke((MethodInvoker)(() =>
                {
                    this.progressBar.Step = 100;
                    this.progressBar.Value = 100;
                }));
                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = "100%";
                }));

                Thread.Sleep(1000);

                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = $"Cliente Extraido. Extrayendo actualizador.";
                }));


                /*
                 * Extraer los archivos del actualizador.
                 * **/
                using (ZipArchive archive = ZipFile.Open(finalUpdaterPath, ZipArchiveMode.Read))
                {

                    var entries = archive.Entries;
                    double percentage = 0;
                    int counter = 0;

                    foreach (ZipArchiveEntry entry in entries)
                    {
                        percentage = (entries.Count() * counter) / 100;
                        this.labelInfo.Invoke((MethodInvoker)(() =>
                        {
                            this.labelInfo.Text = $"{percentage}%";
                        }));
                        // Determine the target file path
                        string containerFilePath = Path.Combine(filesManager.diskPath!, JpConstants.UpdaterFolderName);
                        string targetFilePath = Path.Combine(containerFilePath, entry.Name);
                        // Create the directory if it doesn't exist
                        // Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
                        if (targetFilePath != containerFilePath)
                        {
                            // Extract the entry, overwriting if the file already exists
                            entry.ExtractToFile(targetFilePath, true);
                        }
                        counter++;
                    }
                }
                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = "Archivos extraidos. Verificando base de datos";
                }));
                if (File.Exists(finalDbLocation))
                {
                    labelInfo.Invoke((MethodInvoker)(() =>
                    {
                        labelInfo.Text = "Omitiendo Lite (Ya existe una)";
                    }));
                }
                else
                {
                    labelInfo.Invoke((MethodInvoker)(() =>
                    {
                        labelInfo.Text = "Lite agregado";
                    }));
                    File.Copy(srcSqlite, finalDbLocation);
                }
                this.progressBar.Invoke((MethodInvoker)(() =>
                {
                    this.progressBar.Visible = false;
                }));
                this.shouldRequestLicense = true;
                if (File.Exists(finalZipPath))
                {
                    File.Delete(finalZipPath);
                }
                if (File.Exists(finalUpdaterPath))
                {
                    File.Delete(finalUpdaterPath);
                }
                Thread.Sleep(2000);

                serviceManager.InstallService(serviceExe, JpConstants.ClientServiceName);
                serviceManager.InstallService(updaterExe, JpConstants.UpdaterServiceName);
                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = "Servicio instalado!";
                }));

            }
            catch (Exception ex)
            {
                this.labelInfo.Invoke((MethodInvoker)(() =>
                {
                    this.labelInfo.Text = ex.Message;
                }));
                this.progressBar.Invoke((MethodInvoker)(() =>
                {
                    this.progressBar.Visible = false;
                }));
                this.installBtn.Invoke((MethodInvoker)(() =>
                {
                    this.installBtn.Enabled = true;
                }));
                this.btnSalir.Invoke((MethodInvoker)(() =>
                {
                    this.btnSalir.Enabled = true;
                    this.btnSalir.Visible = true;
                }));
                this.isInstalling = false;
            }
        }

        private void showLicense()
        {
            this.Hide();
            var form2 = new RegisterLicense();
            form2.Show();
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Perform any actions after the background task is completed
            // This code will run on the UI thread
            if (this.shouldRequestLicense)
            {
                this.showLicense();
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            this.removeServices();
        }

        private void removeServices()
        {
            this.btnSalir.Enabled = false;
            this.btnRemove.Enabled = false;
            this.installBtn.Enabled = false;
            this.labelInfo.Text = "Desinstalando, Por favor Espere...";
            this.progressBar.Visible = true;
            this.progressBar.Style = ProgressBarStyle.Marquee;
            this.progressBar.MarqueeAnimationSpeed = 20;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += WorkerUninstall!;
            worker.RunWorkerCompleted += WorkerUninstallComplete!;
            worker.RunWorkerAsync();
        }

        private void WorkerUninstallComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btnRemove.Enabled = true;
            this.cancelarBtn.Enabled = true;
            this.installBtn.Enabled = true;
            this.labelInfo.Text = "";
            this.progressBar.Visible = false;
            this.progressBar.Style = ProgressBarStyle.Continuous;
            this.labelInfo.Text = "Desinstalación completa.";
            this.btnSalir.Enabled = true;

            if (this._isFullRemove)
            {
                this._isFullRemove = false;
                this.labelInfo.Text = "Eliminando contenido de carpetas...";
                string finalApplicationPath = Path.Combine(filesManager.diskPath!, JpConstants.ClientFolderName);
                string containerFilePath = Path.Combine(filesManager.diskPath!, JpConstants.UpdaterFolderName);
                if (Directory.Exists(finalApplicationPath))
                {
                    Directory.Delete(finalApplicationPath, true);
                }
                if (Directory.Exists(containerFilePath))
                {
                    Directory.Delete(containerFilePath, true);
                }
                this.labelInfo.Text = "Completado!";
            }

        }

        private void WorkerUninstall(object sender, DoWorkEventArgs e)
        {
            ServiceInstaller serviceInstaller = new ServiceInstaller(JpConstants.ClientServiceName, "");
            ServiceInstaller serviceUpdater = new ServiceInstaller(JpConstants.UpdaterServiceName, "");
            serviceInstaller.Uninstall();
            serviceUpdater.Uninstall();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.showLicense();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P && e.Control && e.Shift)
            {
                this.btnFullRemove.Visible = !this.btnFullRemove.Visible;
            }
        }

        private void btnFullRemove_Click(object sender, EventArgs e)
        {
            this._isFullRemove = true;
            this.removeServices();
        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }
    }
}
