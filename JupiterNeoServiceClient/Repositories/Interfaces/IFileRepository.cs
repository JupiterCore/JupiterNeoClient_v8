using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Models.Domain;
using BackupFile = JupiterNeoServiceClient.Models.Domain.BackupFile;

namespace JupiterNeoServiceClient.Repositories.Interfaces
{
    internal interface IFileRepository
    {
        Task<BackupFile?> FileByPath(string? filePath);
        Task<bool> FileExists(string filePath);
        Task<bool> InsertFile(BackupFile file);
        Task OnFileModified(string filePath, string updatedAt);
        Task<IEnumerable<BackupFile>> GetBackedUpNull(int batchSize);
        Task<IEnumerable<BackupFile>> GetTotalFiles();
        Task MarkAsBackedUp(int id);
        Task<BackupFile?> GetFileById(int id);
        Task MarkAsFailed(int id, int forcedCount = -1);
        Task <IEnumerable<BackupFile>> ListBackedUp();
        Task DeleteStartsWithPathAndHasBackedUp(string startsWith);
        Task MarkAsDeleted(string filePath);
        Task ResetFailed();
        Task<int> PendingFilesCount();
        Task<int> TotalScannedCount();
    }
}
