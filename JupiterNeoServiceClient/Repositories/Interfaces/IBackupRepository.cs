using JupiterNeoServiceClient.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Repositories.Interfaces
{
    internal interface IBackupRepository
    {
        Task<Backup> AddBackup(Backup backup);
        Task<Backup?> GetBackupById(int id); // Get by field Id
        Task<Backup?> MarkBackupAsScanned(int id);
        Task<Backup?> MarkBackupAsStarted(int id);
        Task<Backup?> MarkBackupAsCompleted(int id);
        Task<Backup?> GetUncompletedBackup();

        Task<Backup?> GetBackupByBackupId(int backupId); // Get by field BackupId
    }
}
