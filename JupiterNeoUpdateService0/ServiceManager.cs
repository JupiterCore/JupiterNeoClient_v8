using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace JupiterNeoUpdateService
{
    public class ServiceManager
    {
        public static void StopAndUninstallService(string serviceName, string executablePath)
        {
            // Stop the service if it is running
            using (ServiceController serviceController = new ServiceController(serviceName))
            {
                if (serviceController.Status != ServiceControllerStatus.Stopped)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }
            }

            // Uninstall the service
            using (AssemblyInstaller installer = GetInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = executablePath;
                installer.Uninstall(null);
            }
        }

        public static void InstallService(string serviceExecutablePath)
        {
            using (AssemblyInstaller installer = GetInstaller())
            {
                installer.UseNewContext = true;
                installer.Path = serviceExecutablePath;
                installer.Install(null);
                installer.Commit(null);
            }
        }

        private static AssemblyInstaller GetInstaller()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return new AssemblyInstaller(assembly, null);
        }
    }
}
