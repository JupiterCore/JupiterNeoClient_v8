using System;
using System.Management;

namespace JpCommon
{
    public static class JpDeviceInfo
    {
        public static void GetDeviceInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                using var information = searcher.Get();

                foreach (var obj in information)
                {
                    if (obj is ManagementObject managementObject)
                    {
                        foreach (var property in managementObject.Properties)
                        {
                            Console.WriteLine("{0} = {1}", property.Name, property.Value ?? "N/A");
                        }
                        Console.WriteLine();
                    }
                }

                Console.WriteLine("Tengo el EM");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo información del dispositivo: {ex.Message}");
            }
        }
    }
}
