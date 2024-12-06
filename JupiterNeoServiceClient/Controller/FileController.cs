using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JpCommon;
using JpCommon;


namespace JupiterNeoServiceClient.Controllers
{
    public class FileController : BaseController
    {
        private readonly FileModel _fileModel;
        private readonly FileProcessor _fileProcessor;

        public FileController(
            FileModel fileModel,
            FileProcessor fileProcessor,
            MetadataModel metaModel,
            JpApi api
        ) : base(metaModel, api)
        {
            _fileModel = fileModel ?? throw new ArgumentNullException(nameof(fileModel));
            _fileProcessor = fileProcessor ?? throw new ArgumentNullException(nameof(fileProcessor));
        }

        public IEnumerable<FModel> GetPendingUploadsBatch(int batchSize = 10)
        {
            return _fileModel.GetBackedUpNull(batchSize);
        }

        public int TotalFilesInDb()
        {
            return _fileModel.TotalScannedCount();
        }

        public string GetDeletedFolder(string path)
        {
            try
            {
                var folders = path.Split(Path.DirectorySeparatorChar);
                var currentPath = string.Empty;

                foreach (var folder in folders)
                {
                    currentPath = Path.Combine(currentPath, folder);

                    if (!Directory.Exists(currentPath))
                    {
                        return Path.GetDirectoryName(currentPath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error detecting deleted folder");
            }

            return null;
        }

        public async Task UploadFileAsync(FModel file, string backupId)
        {
            try
            {
                if (!File.Exists(file.FilePath))
                {
                    if (file.FileBackedUp == "1")
                    {
                        await ReportDeletedFileAsync(backupId, file.FilePath);
                    }

                    var deletedFolder = GetDeletedFolder(file.FilePath);
                    if (deletedFolder != null)
                    {
                        _fileModel.DeleteStartsWithPathAndHastBackedUp(deletedFolder);
                    }
                    return;
                }

                if (FileProcessor.FileCanBeRead(file.FilePath))
                {
                    var response = await Api.UploadFileAsync(License, backupId, file.FilePath, file.FilePath);
                    response.EnsureSuccessStatusCode();
                    _fileModel.MarkAsBackedUp(file.FileId);
                }
                else
                {
                    _fileModel.MarkAsFailed(file.FileId);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error uploading file");
                _fileModel.MarkAsFailed(file.FileId);
            }
        }

        public async Task UploadBatchAsync(string backupId)
        {
            foreach (var file in GetPendingUploadsBatch())
            {
                if (await Api.IsServerResponseOkAsync())
                {
                    await UploadFileAsync(file, backupId);
                }
                else
                {
                    break;
                }
            }
        }

        public async Task ReportDeletedFilesAsync(string backupId)
        {
            foreach (var file in _fileModel.ListBackedUp())
            {
                if (!File.Exists(file.FilePath))
                {
                    await ReportDeletedFileAsync(backupId, file.FilePath);
                }
            }
        }

        public async Task ReportDeletedFileAsync(string backupId, string filePath)
        {
            var response = await Api.ReportDeletedFileAsync(License, backupId, filePath);
            response.EnsureSuccessStatusCode();
            _fileModel.MarkAsDeleted(filePath);
        }

        public void ResetFailed()
        {
            _fileModel.ResetFailed();
        }

        public int GetPendingFilesCount()
        {
            return _fileModel.PendingFilesCount();
        }
    }
}
