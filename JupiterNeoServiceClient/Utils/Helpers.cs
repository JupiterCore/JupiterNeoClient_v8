using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;

namespace JupiterNeoServiceClient.classes
{

    public static class Helpers
    {
        public static string today()
        {
            DateTime currentDate = DateTime.Now;
            return currentDate.ToString("yyyy-MM-dd");
        }

        public static List<string> DirSearch(string sDir, string[] extensions)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    if (extensions.Contains(Path.GetExtension(f)))
                    {
                        files.Add(f);
                    }
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    try
                    {
                        // Exclude temporary folders and folders that are not worth scanning
                        if (IsExcludedFolder(d))
                        {
                            continue;
                        }

                        files.AddRange(DirSearch(d, extensions));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Handle or ignore the unauthorized access exception for the directory
                        // Here, we're simply skipping the inaccessible directory
                        continue;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle or ignore the unauthorized access exception for the current directory
                return files; // Returning the files collected so far
            }
            catch (Exception)
            {
            }

            return files;
        }

        private static bool IsExcludedFolder(string folderPath)
        {
            string folderName = Path.GetFileName(folderPath);

            // Exclude temporary folders based on specific criteria
            if (folderName.StartsWith("~") || folderName.StartsWith("."))
            {
                return true;
            }

            // Compare the full path of the "Temp" folder with a specific path
            string tempFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Temp");

            if (String.Equals(folderPath, tempFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            string localTemp = @"Local\Temp";
            string localLowTemp = @"LocalLow\Temp";

            if (folderPath.Contains(localTemp) || folderPath.Contains(localLowTemp))
            {
                return true;
            }

            // Compare the full path of the "Cache" folder with a specific path
            string cacheFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Cache");
            if (String.Equals(folderPath, cacheFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }


            // Compare the full path of the local AppData folder with a specific path
            string localAppDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (String.Equals(folderPath, localAppDataFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Compare the full path of the roaming AppData folder with a specific path
            string roamingAppDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (String.Equals(folderPath, roamingAppDataFolderPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static bool HasTimeElapsed(string targetTime)
        {
            // Parse the target time string
            TimeSpan target;
            if (!TimeSpan.TryParse(targetTime, out target))
            {
                throw new ArgumentException("Invalid target time format.");
            }

            // Get the current time
            TimeSpan currentTime = DateTime.Now.TimeOfDay;

            // Compare the target time with the current time and return the result
            return currentTime >= target;
        }

        public static bool canPingServer()
        {
            try
            {
                Ping myPing = new Ping();
#if DEBUG
                String host = "http://localhost:8080";
#else
                String host = "https://api.jupiterneo.cloud";
#endif
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                return (reply.Status == IPStatus.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }

    }


}
