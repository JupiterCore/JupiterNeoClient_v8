using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;

namespace Installer
{
    public class UserServiceInstaller
    {
        private const string ServiceRegistryKey = @"SYSTEM\CurrentControlSet\Services";
        private const string UserInitKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";
        private const string ServiceImagePathValue = "ImagePath";

        public static bool InstallService(string serviceName, string serviceExePath)
        {
            if (IsServiceInstalled(serviceName))
            {
                if (!StopService(serviceName))
                {
                    return false;
                }

                if (!UninstallService(serviceName))
                {
                    return false;
                }
            }

            try
            {
                // Create a registry key for the service under CurrentControlSet\Services
                using (RegistryKey serviceKey = Registry.LocalMachine.CreateSubKey(Path.Combine(ServiceRegistryKey, serviceName)))
                {
                    // Set the service's executable path as ImagePath value
                    serviceKey.SetValue(ServiceImagePathValue, serviceExePath);
                }

                // Set the logged-in user's startup program to launch the service executable
                using (RegistryKey userInitKey = Registry.CurrentUser.CreateSubKey(UserInitKey))
                {
                    userInitKey.SetValue("Userinit", serviceExePath);
                }

                // Start the service to complete the installation
                using (ServiceController serviceController = new ServiceController(serviceName))
                {
                    serviceController.Start();
                    serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsServiceInstalled(string serviceName)
        {
            using (RegistryKey serviceKey = Registry.LocalMachine.OpenSubKey(Path.Combine(ServiceRegistryKey, serviceName)))
            {
                return serviceKey != null;
            }
        }

        public static bool StopService(string serviceName)
        {
            try
            {
                using (ServiceController serviceController = new ServiceController(serviceName))
                {
                    if (serviceController.Status != ServiceControllerStatus.Stopped)
                    {
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UninstallService(string serviceName)
        {
            try
            {
                // Use sc.exe command-line tool to delete the service
                Process process = new Process();
                process.StartInfo.FileName = "sc.exe";
                process.StartInfo.Arguments = $"delete \"{serviceName}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                // Wait for the process to exit
                process.WaitForExit();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
