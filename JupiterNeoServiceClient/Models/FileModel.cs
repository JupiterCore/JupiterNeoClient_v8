using JupiterNeoServiceClient.classes;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{
    public class FileModel
    {
        public string? FilePath { get; set; }
        public string? FileAddedAt { get; set; }
        public string? FileCreatedAt { get; set; }
        public string? FileUpdatedAt { get; set; }
        public string? FileDeletedAt { get; set; }
        public string? FileBackedUp { get; set; }
        public int FileFailedAttempts { get; set; }
    }

    public class FModel : FileModel
    {
        public int FileId { get; set; }
    }

    public class FileModel : BaseModel
    {
        public enum Fields
        {
            ID,
            PATH,
            ADDED_AT,
            CREATED_AT,
            UPDATED_AT,
            DELETED_AT,
            BACKED_UP,
            FAILED_ATTEMPTS
        }

        public FileModel()
        {
            TableName = "file";

            fields = new Dictionary<Enum, string>
            {
                [Fields.ID] = "file_id",
                [Fields.PATH] = "file_path",
                [Fields.ADDED_AT] = "file_added_at",
                [Fields.CREATED_AT] = "file_created_at",
                [Fields.UPDATED_AT] = "file_updated_at",
                [Fields.DELETED_AT] = "file_deleted_at",
                [Fields.BACKED_UP] = "file_backed_up",
                [Fields.FAILED_ATTEMPTS] = "file_failed_attempts"
            };
        }

        public FModel? FileByPath(string filePath) =>
            query().Where(fields[Fields.PATH], filePath).Get<FModel>().FirstOrDefault();

        public bool FileExists(string filePath) =>
            FileByPath(filePath) != null;

        public bool InsertFile(string filePath, string createdAt, string updatedAt)
        {
            var model = new FileModel
            {
                FilePath = filePath,
                FileAddedAt = Helpers.Today(),
                FileCreatedAt = createdAt,
                FileUpdatedAt = updatedAt
            };

            return insert(model) > 0;
        }

        public int OnFileModified(string filePath, string updatedAt)
        {
            var file = FileByPath(filePath) ?? throw new InvalidOperationException($"File not found: {filePath}");
            file.FileUpdatedAt = updatedAt;
            file.FileBackedUp = null;
            file.FileDeletedAt = null;

            return Execute(query().Where(fields[Fields.ID], file.FileId).AsUpdate(file));
        }

        public IEnumerable<FModel> GetBackedUpNull(int batchSize) =>
            query().WhereNull(fields[Fields.BACKED_UP]).Limit(batchSize).Get<FModel>();

        public IEnumerable<FModel> GetTotalFiles() =>
            query().Get<FModel>();

        public void MarkAsBackedUp(int id)
        {
            var data = new Dictionary<string, object?>
            {
                [fields[Fields.BACKED_UP]] = 1
            };

            Execute(query().Where(fields[Fields.ID], id).AsUpdate(data));
        }

        public FModel? GetFileById(int id) =>
            query().Where(fields[Fields.ID], id).Get<FModel>().FirstOrDefault();

        public void MarkAsFailed(int id, int forcedCount = -1)
        {
            var file = GetFileById(id) ?? throw new InvalidOperationException($"File not found: {id}");
            var failedCount = forcedCount >= 0 ? forcedCount : file.FileFailedAttempts + 1;

            var data = new Dictionary<string, object?>
            {
                [fields[Fields.FAILED_ATTEMPTS]] = failedCount
            };

            if (failedCount >= 3)
            {
                // Si falla más de 3 veces, marcar como respaldado.
                data[fields[Fields.BACKED_UP]] = 1;
            }

            Execute(query().Where(fields[Fields.ID], id).AsUpdate(data));
        }

        public IEnumerable<FModel> ListBackedUp() =>
            query()
                .Where(fields[Fields.BACKED_UP], 1)
                .WhereNull(fields[Fields.DELETED_AT])
                .Get<FModel>();

        public void DeleteStartsWithPathAndHasBackedUp(string startsWith) =>
            query()
                .WhereStarts(fields[Fields.PATH], startsWith)
                .Delete();

        public int MarkAsDeleted(string filePath)
        {
            var file = FileByPath(filePath) ?? throw new InvalidOperationException($"File not found: {filePath}");
            file.FileDeletedAt = Helpers.Today();

            return Execute(query().Where(fields[Fields.ID], file.FileId).AsUpdate(file));
        }

        public void ResetFailed()
        {
            var updateData = new Dictionary<string, object?>
            {
                [fields[Fields.BACKED_UP]] = null,
                [fields[Fields.FAILED_ATTEMPTS]] = 0
            };

            Execute(query().Where(fields[Fields.FAILED_ATTEMPTS], ">", 0).AsUpdate(updateData));
        }

        public int PendingFilesCount() =>
            query().WhereNull(fields[Fields.BACKED_UP]).AsCount().FirstOrDefault<int>();

        public int TotalScannedCount() =>
            query().AsCount().FirstOrDefault<int>();
    }
}
