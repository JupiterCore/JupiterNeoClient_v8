using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace JpCommon
{
    public class BackupSchedule
    {
        public string[]? paths { get; set; }
        public string[]? schedules { get; set; }
    }

    public class JpApi : BaseHttp
    {

        public JpApi()
        {
            this.baseURL = JpConstants.ApiBaseUrl;
        }

        public JpApi(string baseURL)
        {
            this.baseURL = baseURL;
        }

        public async Task<BackupSchedule> getSchedulesAsync(string license)
        {
            BackupSchedule sc = new BackupSchedule();
            HttpResponseMessage response = await this.get($"/schedule/{license}");
            response.EnsureSuccessStatusCode();
            var responseObject = await this.getJSON(response);

            if (responseObject != null)
            {
                JArray schedules = responseObject.data.backups;
                JArray paths = responseObject.data.paths;
                if (schedules != null)
                {
                    sc.paths = paths.ToObject<string[]>();
                    sc.schedules = schedules.ToObject<string[]>();
                }
            }
            else
            {
                string[] emptySchedules = new string[0];
                sc.schedules = emptySchedules;

            }
            return sc;
        }

#nullable enable
        public async Task<string[]> getExtensions(string license)
        {
            string[] list = Array.Empty<string>(); // Use Array.Empty<string>() for clarity and efficiency
            try
            {
                var result = await this.get($"/extension/license/{license}");
                result.EnsureSuccessStatusCode();

                var responseObject = await this.getJSON(result);
                if (responseObject?.data?.list != null)
                {
                    // Safely cast to JArray and convert to string[]
                    JArray data = responseObject.data.list;
                    list = data.ToObject<string[]>() ?? Array.Empty<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching extensions: {ex.Message}");
            }

            return list;
        }

        public async Task<int?> GetBackupIdV2Async(string license)
        {
            var result = await this.get($"/backup/{license}");
            result.EnsureSuccessStatusCode();
            var responseObject = await this.getJSON(result);

            if (responseObject?.backupId != null)
            {
                if (int.TryParse(responseObject.backupId.ToString(), out int backupId))
                {
                    return backupId;
                }
            }
            return null;
        }


        public async Task<HttpResponseMessage> UpdateScannedFilesForBackup(string license, int backupId, int totalScannedFiles)
        {
            object data = new
            {
                scannedFiles = totalScannedFiles
            };
            
            return await this.patch($"/backup/expected-files/{license}/{backupId}", data);
        }


        public async Task<string?> getBackupId(string license, int expectedFiles)
        {
            try
            {
                object data = new
                {
                    expected_files = expectedFiles
                };

                var result = await this.post($"/bucket/start/{license}", data);
                result.EnsureSuccessStatusCode();

                var responseObject = await this.getJSON(result);
                if (responseObject?.backupId != null)
                {
                    return responseObject.backupId.ToString(); // Ensure we safely convert it to a string
                }

                Console.WriteLine("backupId is null in the response.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting backup ID: {ex.Message}");
                return null;
            }
        }

        public async Task<HttpResponseMessage> uploadFile(string license, string backupId, string filePath, string readFromPath)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var formData = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(File.ReadAllBytes(readFromPath));
                    formData.Add(fileContent, "file", Path.GetFileName(readFromPath));
                    var fileInfo = new FileInfo(filePath);
                    DateTime createdAt = fileInfo.CreationTime;
                    DateTime updatedAt = fileInfo.LastWriteTime;
                    string dateFormat = "yyyy-MM-dd HH:mm:ss";
                    formData.Add(new StringContent(filePath), "file_path");
                    formData.Add(new StringContent(createdAt.ToString(dateFormat)), "created_at");
                    formData.Add(new StringContent(updatedAt.ToString(dateFormat)), "updated_at");
                    HttpResponseMessage response = await client.PostAsync($"{this.baseURL}/bucket/{backupId}/{license}", formData);
                    return response;
                }
            }
        }

        public async Task<HttpResponseMessage> concludeBackup(string license, string backupId)
        {

            object data = new
            {
            };
            return await this.post($"/bucket/conclude/{backupId}/{license}", data);
        }

        public async Task<HttpResponseMessage> reportDeletedFile(string license, string backupId, string path)
        {
            object data = new
            {
                backupId = backupId,
                path = path
            };
            return await this.post($"/bucket/deleted/{license}", data);
        }

        public async Task<HttpResponseMessage> NotifyInstalledPrograms(string license, List<string> programs)
        {
            object data = new
            {
                license = license,
                programs = programs
            };
            return await this.post("/programs/", data);
        }

        public async Task<bool> isServerResponseOk()
        {
            try
            {
                var response = await this.get("/ping");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<HttpResponseMessage> notifyVersion(string license, string version)
        {
            object data = new
            {
                license = license,
                version = version
            };
            return await this.post("/computer/client-version/", data);
        }

        public async Task<HttpResponseMessage> notifyPathsAvailable(string? license)
        {
            object data = new
            {
                license = license,
                paths_available = JpPaths.listAvailableBackupPaths()
            };
            return await this.post("/computer/available-paths", data);
        }

        public async Task<HttpResponseMessage> notifyDisksAvailable(string license, List<string> drivesFound)
        {
            object data = new
            {
                drives_found = drivesFound
            };
            return await this.post($"/computer/report-drives/{license}", data);
        }
    }

}
  