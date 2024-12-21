using System;
using System.Collections.Generic;
using System.IO;

namespace JpCommon
{
    public static class JpPaths
    {


        public static List<string> ListSubPaths(string folderPath)
        {
            List<string> subfolders = new List<string>();
            if (!string.IsNullOrEmpty(folderPath))
            {
                DirectoryInfo[] subDirectories = new DirectoryInfo(folderPath).GetDirectories();
                foreach (DirectoryInfo subDir in subDirectories)
                {
                    subfolders.Add(subDir.FullName);
                }
            }
            return subfolders;
        }
        public static string GetMainDrive()
        {
            // Get all available drives on the system
            DriveInfo[] drives = DriveInfo.GetDrives();

            // Iterate through the drives and find the one with the OS installed
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    return drive.Name; // Return the drive letter (e.g., "C:\")
                }
            }

            // Return an empty string if no valid drive is found
            return string.Empty;
        }

        public static List<string> GetAllFixedDrives()
        {
            List<string> fixedDrives = new List<string>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Fixed)
                {
                    fixedDrives.Add(drive.Name);
                }
            }
            return fixedDrives;
        }

        public static List<string> listAvailableBackupPaths()
        {
            var fixedDrives = GetAllFixedDrives();
            var allSubfolders = new List<string>();

            fixedDrives.ForEach(fixedDrive =>
            {
                var subfoldersInDrive = ListSubPaths(fixedDrive);
                subfoldersInDrive.ForEach(path =>
                {
                    allSubfolders.Add(path);
                });
            });
            return allSubfolders;
        }

        public static string CreateTemporaryPath()
        {
            string tempFolderPath = Path.GetTempPath();
            string uniqueId = Guid.NewGuid().ToString("N");
            string temporaryPath = Path.Combine(tempFolderPath, uniqueId);
            Directory.CreateDirectory(temporaryPath);
            return temporaryPath;
        }

    }
}


