using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Models.Domain;

namespace JupiterNeoServiceClient.Repositories.Interfaces
{
    internal interface IBackupPathRepository
    {
        Task<BackupPaths?> ByPath(string path);
        Task<bool> ExistsPath(string path);
        Task<List<BackupPaths>> GetAllPaths();
        Task DeleteByPath(string path);
        Task AddPath(string path);
    }
}
