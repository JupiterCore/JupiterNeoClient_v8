using JpCommon;
using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CommonClasessLibrary
{
    public class Downloader
    {
        public bool isDownloadAvailable { get; set; }
        public string zipName = "";
        public string exeName = "";
        public string version = "";

        public async Task checkDownloadLink()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(JpConstants.UpdaterBaseUrl + "/updates/latest");
            response.EnsureSuccessStatusCode(); // Ensure the response is successful (status code 200)

            // Deserialize JSON response using System.Text.Json
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LatestUpdate>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result != null)
            {
                if (result.exe_name != null)
                {
                    exeName = result.exe_name;
                }

                if (result.file != null)
                {
                    zipName = result.file;
                }

                if (result.version != null)
                {
                    version = result.version;
                }

                isDownloadAvailable = result.file != null;
            }
        }

        public async Task<HttpResponseMessage> startDownload(string downloadURL)
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromHours(4); // Set timeout to 4 hours or as needed
            HttpResponseMessage fileResponse = await client.GetAsync(downloadURL);
            fileResponse.EnsureSuccessStatusCode(); // Ensure the response is successful (status code 200)
            return fileResponse;
        }

        public async Task writeDownload(HttpResponseMessage fileResponse, string destinationPath, Progress<long>? progress)
        {
            // Get the file content as a stream
            using (Stream contentStream = await fileResponse.Content.ReadAsStreamAsync())
            {
                // Create a FileStream to save the file
                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                {
                    // Create a buffer to read the data in chunks
                    byte[] buffer = new byte[8192];
                    int bytesRead;
                    long totalBytesRead = 0;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        // Write the downloaded data to the file stream
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        // Update the total bytes read and report progress
                        totalBytesRead += bytesRead;
                        if (progress != null)
                        {
                            ((IProgress<long>)progress).Report(totalBytesRead);
                        }
                    }
                }
            }
        }
    }
}
