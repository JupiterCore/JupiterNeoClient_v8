using System;
using System.Diagnostics;
using System.ServiceProcess;


namespace JpCommon
{
    public class JpServiceManager
    {
        public JpServiceManager()
        {
        }

        public bool InstallService(string exePath, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(exePath))
                throw new ArgumentException("Executable path cannot be null or empty.", nameof(exePath));
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            try
            {
#if WINDOWS
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"create {serviceName} binPath=\"{exePath}\" start=auto",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
#else
                Console.WriteLine("Service management is only available on Windows.");
                return false;
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing service: {ex.Message}");
                return false;
            }
        }

        public bool StartService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            try
            {
#if WINDOWS
                using Process startProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"start {serviceName}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                startProcess.Start();
                startProcess.WaitForExit();
                return startProcess.ExitCode == 0;
#else
                Console.WriteLine("Service management is only available on Windows.");
                return false;
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting service: {ex.Message}");
                return false;
            }
        }

        public bool UninstallService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            try
            {
#if WINDOWS
                using Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "sc.exe",
                        Arguments = $"delete {serviceName}",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();
                return process.ExitCode == 0;
#else
                Console.WriteLine("Service management is only available on Windows.");
                return false;
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uninstalling service: {ex.Message}");
                return false;
            }
        }

        public bool StopService(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            try
            {
#if WINDOWS
                using ServiceController serviceController = new ServiceController(serviceName);
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                }

                return true;
#else
                Console.WriteLine("Service management is only available on Windows.");
                return false;
#endif
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping service: {ex.Message}");
                return false;
            }
        }

        public bool IsServiceInstalled(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            try
            {
#if WINDOWS
                using ServiceController serviceController = new ServiceController(serviceName);
                ServiceControllerStatus status = serviceController.Status;
                return true;
#else
                Console.WriteLine("Service management is only available on Windows.");
                return false;
#endif
            }
            catch
            {
                return false;
            }
        }

        public bool FullUninstall(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Service name cannot be null or empty.", nameof(serviceName));

            if (IsServiceInstalled(serviceName))
            {
                if (StopService(serviceName))
                {
                    return UninstallService(serviceName);
                }

                return false;
            }

            return true; // Service is not installed, considered as uninstalled.
        }
    }
}
