using System.Management;

namespace Installer
{
    public class SystemInfo
    {

        public static Dictionary<string, object> GetSystemInfo()
        {
            Dictionary<string, object> info = new Dictionary<string, object>
            {
                { "cpus", GetCpus() },
                { "ram", GetTotalMemory() },
                { "platform", GetPlatform() },
                { "osVersion", Environment.OSVersion}
            };
            return info;
        }

        private static List<object> GetCpus()
        {
            var cpus = new List<object>();
            var managementClass = new ManagementClass("Win32_Processor");
            var managementObjects = managementClass.GetInstances();

            foreach (var obj in managementObjects)
            {
                var cpu = new
                {
                    Name = obj["Name"],
                    Manufacturer = obj["Manufacturer"],
                    NumberOfCores = obj["NumberOfCores"],
                    MaxClockSpeed = obj["MaxClockSpeed"]
                };

                cpus.Add(cpu);
            }

            return cpus;
        }

        private static ulong GetTotalMemory()
        {
            var managementClass = new ManagementClass("Win32_ComputerSystem");
            var managementObject = managementClass.GetInstances().Cast<ManagementObject>().FirstOrDefault();

            if (managementObject != null)
            {
                var totalMemory = managementObject["TotalPhysicalMemory"];
                return Convert.ToUInt64(totalMemory);
            }

            return 0;
        }

        private static string GetPlatform()
        {
            return Environment.OSVersion.Platform.ToString();
        }

        private static string GetOsVersion()
        {
            var osVersion = Environment.OSVersion.Version;
            return $"{osVersion.Major}.{osVersion.Minor}";
        }
    }

}
 