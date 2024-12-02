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
        public string[] Paths { get; set; } = Array.Empty<string>();
        public string[] Schedules { get; set; } = Array.Empty<string>();
    }

    public class JpApi : BaseHttp
    {
        public JpApi()
        {
            baseURL = JpConstants.ApiBaseUrl;
        }

        public JpApi(string baseURL)
        {
            this.baseURL = baseURL;
        }

        public async Task<BackupSchedule> GetSchedulesAsync(string license)
        {
            var schedule = new BackupSchedule();
            try
            {
                HttpResponseMessage response = await Get($"/schedule/{license}");
                response.EnsureSuccessStatusCode();

                var responseObject = await GetJSON(response);

                if (responseObject != null)
                {
                    JArray schedules = responseObject.data?.backups ?? new JArray();
                    JArray paths = responseObject.data?.paths ?? new JArray();

                    schedule.Paths = paths.ToObject<string[]>() ?? Array.Empty<string>();
                    schedule.Schedules = schedules.ToObject<string[]>() ?? Array.Empty<string>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching schedules: {ex.Message}");
            }

            return schedule;
        }

        public async Task<string[]> GetExtensionsAsync(string license)
        {
            try
            {
                HttpResponseMessage response = await Get($"/extension/license/{license}");
                response.EnsureSuccessStatusCode();

                var responseObject = await GetJSON(response);
                JArray data = responseObject.data?.list ?? new JArray();

                return data.ToObject<string[]>() ?? Array.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching extensions: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        public async Task<string?> GetBackupIdAsync(string license, int expectedFiles)
        {
            try
            {
                var data = new { expected_files = expectedFiles };
                HttpResponseMessage response = await Post($"/bucket/start/{license}", data);
                response.EnsureSuccessStatusCode();

                var responseObject = await GetJSON(response);
                return responseObject.backupId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting backup ID: {ex.Message}");
                return null;
            }
        }

        public async Task<HttpResponseMessage> UploadFileAsync(string license, string backupId, string filePath, string readFromPath)
        {
            using var client = new HttpClient();
            using var formData = new MultipartFormDataContent();

            try
            {
                var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(readFromPath));
                formData.Add(fileContent, "file", Path.GetFileName(readFromPath));

                var fileInfo = new FileInfo(filePath);
                const string dateFormat = "yyyy-MM-dd HH:mm:ss";

                formData.Add(new StringContent(filePath), "file_path");
                formData.Add(new StringContent(fileInfo.CreationTime.ToString(dateFormat)), "created_at");
                formData.Add(new StringContent(fileInfo.LastWriteTime.ToString(dateFormat)), "updated_at");

                return await client.PostAsync($"{baseURL}/bucket/{backupId}/{license}", formData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file: {ex.Message}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> ConcludeBackupAsync(string license, string backupId)
        {
            var data = new { };
            return await Post($"/bucket/conclude/{backupId}/{license}", data);
        }

        public async Task<HttpResponseMessage> ReportDeletedFileAsync(string license, string backupId, string path)
        {
            var data = new { backupId, path };
            return await Post($"/bucket/deleted/{license}", data);
        }

        public async Task<HttpResponseMessage> NotifyInstalledProgramsAsync(string license, List<string> programs)
        {
            var data = new { license, programs };
            return await Post("/programs/", data);
        }

        public async Task<bool> IsServerResponseOkAsync()
        {
            try
            {
                HttpResponseMessage response = await Get("/ping");
                response.EnsureSuccessStatusCode();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<HttpResponseMessage> NotifyVersionAsync(string license, string version)
        {
            var data = new { license, version };
            return await Post("/computer/client-version/", data);
        }

        public async Task<HttpResponseMessage> NotifyPathsAvailableAsync(string license)
        {
            var data = new
            {
                license,
                paths_available = JpPaths.ListAvailableBackupPaths()
            };
            return await Post("/computer/available-paths", data);
        }

        public async Task<HttpResponseMessage> NotifyDisksAvailableAsync(string license, List<string> drivesFound)
        {
            var data = new { drives_found = drivesFound };
            return await Post($"/computer/report-drives/{license}", data);
        }
    }
}

