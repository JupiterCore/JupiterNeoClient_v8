using JupiterNeoServiceClient.Data;
using JupiterNeoServiceClient.Models.Domain;
using JupiterNeoServiceClient.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JupiterNeoServiceClient.Repositories.Implementations
{
    internal class SQLiteBackupRepository : IBackupRepository
    {
        private readonly NeoDbContext dbContext;

        public SQLiteBackupRepository(NeoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<Backup> AddBackup(Backup backup)
        {
            await this.dbContext.Backups.AddAsync(backup);
            await this.dbContext.SaveChangesAsync();
            return backup;
        }

        public async Task<Backup?> GetBackupByBackupId(int backupId)
        {
            return await this.dbContext.Backups.FirstOrDefaultAsync(b => b.BackupId == backupId);
        }

        public async Task<Backup?> GetBackupById(int id)
        {
            var backup = await this.dbContext.Backups.FirstOrDefaultAsync(b=>b.Id == id);
            return backup;
        }

        public async Task<Backup?> GetUncompletedBackup()
        {
            var backup = await this.dbContext.Backups.FirstOrDefaultAsync(b=>b.IsCompleted == 0);
            return backup;
        }

        public async Task<Backup?> MarkBackupAsCompleted(int id)
        {
            var backup = await this.dbContext.Backups.FirstOrDefaultAsync(b => b.Id == id);

            if (backup == null)
            {
                return null;
            }

            backup.IsCompleted = 1;
            await this.dbContext.SaveChangesAsync();

            return backup;
        }

        public async Task<Backup?> MarkBackupAsScanned(int id)
        {
            var backup = await this.dbContext.Backups.FirstOrDefaultAsync(b => b.Id == id);
            if (backup == null)
            {
                return null;
            }
            backup.IsScanned = 1;
            await this.dbContext.SaveChangesAsync();
            return backup;
        }

        public async Task<Backup?> MarkBackupAsStarted(int id)
        {
            var backup = await this.dbContext.Backups.FirstOrDefaultAsync(b=> b.Id == id);
            if (backup == null)
            {
                return null;
            }
            backup.IsStarted = 1;
            await this.dbContext.SaveChangesAsync();
            return backup;
        }
    }
}
