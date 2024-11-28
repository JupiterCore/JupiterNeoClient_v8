using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Utils
{
    public class FileManager
    {

        public string AppData = "";
        public readonly string JpFolderName = "JupiterNeoClient";
        public string AppContainer = "";

        public FileManager()
        {

#if DEBUG
            this.AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            this.AppContainer = Path.Combine(this.AppData, this.JpFolderName);
#else
            this.AppData = AppDomain.CurrentDomain.BaseDirectory;
            this.AppContainer = AppDomain.CurrentDomain.BaseDirectory;
#endif
        }

        public string getCurrentVersion()
        {

            var versionPath = Path.Combine(this.AppContainer, "version.txt");
            string currentVersion;
            try
            {
                using (StreamReader reader = new StreamReader(versionPath))
                {
                    currentVersion = reader.ReadLine() ?? "";
                }
            }
            catch (Exception)
            {
                currentVersion = "N/A";
            }
            return currentVersion;
        }

        public string WriteToFile(bool isFirst)
        {
            try
            {
                DateTime currentDate = DateTime.Now;
                string formattedDate = currentDate.ToString("dddd - d MMM yyyy @h:mm tt");

                if (isFirst)
                {
                    formattedDate += "- Desde Comienzo";
                }
                else
                {
                    formattedDate += "- Desde Loop";
                }

                formattedDate += Environment.NewLine;

                string baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string folderPath = Path.Combine(baseFolder, "TXT");
                string tempPath = Path.Combine(folderPath, "test-v2.txt");


                if (!File.Exists(tempPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (File.Exists(tempPath))
                {
                    File.AppendAllText(tempPath, formattedDate);
                }
                else
                {
                    // Create a new file
                    using (FileStream fs = File.Create(tempPath))
                    {
                        // Add some text to file    
                        byte[] author = new UTF8Encoding(true).GetBytes(formattedDate);
                        fs.Write(author, 0, author.Length);
                    }
                }
                return tempPath;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
