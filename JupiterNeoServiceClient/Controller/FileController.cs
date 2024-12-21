using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JpCommon;


namespace JupiterNeoServiceClient.Controllers
{

    public class FileController : BaseController
    {
        public FileModel model = new FileModel();
        public FileProcessor fileProcessor = new FileProcessor();
        public IEnumerable<FModel> getPendingUploadsBatch()
        {
            int batchSize = 10;
            return model.getBackedUpNull(batchSize);
        }

        public int totalFilesInDb()
        {
            return this.model.totalScannedCount();
        }


        public string GetDeletedFolder(string path)
        {
            string[] folders = path.Split('\\'); // Split the path by folder separator
            string currentPath = string.Empty;

            int folderCounter = 0;

            foreach (string folder in folders)
            {
                var append = folderCounter == folders.Length - 1 ? "" : "\\";
                folderCounter++;

                currentPath = Path.Combine(currentPath, folder + append); // Build the current path

                if (File.Exists(currentPath))
                {
                    return null;
                }
                // Check if the folder exists, if not return the previous path
                if (!Directory.Exists(currentPath))
                {
                    return Path.GetDirectoryName(currentPath).Replace("\\", @"\");
                }
            }
            return null; // Return null if no folder was deleted
        }

        public async Task uploadFile(FModel file, string backupId)
        {
            try
            {
                string readFrom = file.file_path;
                if (!File.Exists(file.file_path))
                {

                    if (file.file_backed_up == "1")
                    {
                        // report deleted only when the file has previously been backed up.
                        try
                        {
                            await this.reportDeletedFile(backupId, file.file_path);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex, "---vvvv-A--");
                        }
                    }

                    // Detect if a deleted subfolder is the cause of the deletion of the file.
                    // In that case more than a file could have been deleted (could be thousands of files) and if they haven't been uploaded we can simply remove them.
                    var deletedPath = GetDeletedFolder(file.file_path);
                    if (deletedPath != null)
                    {
                        // A folder was deleted. Delete all files in the database that belonged to that folder and that haven't been backed up.
                        this.model.deleteStartsWithPathAndHastBackedUp(deletedPath);
                    }
                }

                if (FileProcessor.FileCanBeRead(file.file_path))
                {
                    try
                    {
                        var response = await this.api.uploadFile(this.license, backupId, file.file_path, readFrom);
                        response.EnsureSuccessStatusCode();
                        this.model.markAsBackedUp(file.file_id);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex, "--vvvv--001--");
                        this.model.markAsFailed(file.file_id);
                    }
                }
                else
                {
                    // NOTE: Here we would previously try to create a shadow copy (VSS) but it would create some issues in the user's machine. That code got removed.
                    this.model.markAsFailed(file.file_id);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "---vvvv-03--");
            }
        }

        public async Task uploadBatch(string backupId)
        {
            IEnumerable<FModel>? filesToUpload = this.getPendingUploadsBatch();
            foreach (var fileToUpload in filesToUpload)
            {
                var crs = await this.api.isServerResponseOk();
                if (crs)
                {
                    await this.uploadFile(fileToUpload, backupId);
                }
                else
                {
                    break;
                }
            }
            filesToUpload = null;
        }

        public async Task reportDeletedFiles(string? backupId)
        {
            try
            {
                if (backupId != null)
                {
                    var allFiles = this.model.listBackedup();

                    foreach (var f in allFiles)
                    {
                        if (!File.Exists(f.file_path) && f.file_path != null)
                        {
                            await this.reportDeletedFile(backupId, f.file_path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "---vvvv-04--");
            }
        }

        public async Task reportDeletedFile(string backupId, string filePath)
        {
            var response = await this.api.reportDeletedFile(this.license, backupId, filePath);
            response.EnsureSuccessStatusCode();
            this.model.markAsDeleted(filePath);
        }

        public void resetFailed()
        {
            this.model.resetFailed();
        }

        public int getPendingFilesCount()
        {
            return this.model.pendingFilesCount();
        }

    }


}
