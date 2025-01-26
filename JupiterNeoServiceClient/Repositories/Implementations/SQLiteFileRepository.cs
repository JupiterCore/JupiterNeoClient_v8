using JupiterNeoServiceClient.Data;
using JupiterNeoServiceClient.Repositories.Interfaces;
using BackupFile = JupiterNeoServiceClient.Models.Domain.BackupFile;
using Microsoft.EntityFrameworkCore;
using EFCore.BulkExtensions;

namespace JupiterNeoServiceClient.Repositories.Implementations
{
    internal class SQLiteFileRepository : IFileRepository
    {
        private readonly NeoDbContext dbContext;

        public SQLiteFileRepository(NeoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task DeleteStartsWithPathAndHasBackedUp(string startsWith)
        {
            // Validate the input parameter
            if (string.IsNullOrEmpty(startsWith))
            {
                throw new ArgumentException("The 'startsWith' parameter cannot be null or empty.", nameof(startsWith));
            }
            var filesToDelete = await dbContext.Files.Where(file => ((file.Path != null && file.Path.StartsWith(startsWith)) && file.FileBackedUp == 1)).ToListAsync();
            dbContext.Files.RemoveRange(filesToDelete);
            await dbContext.SaveChangesAsync();
        }

        public async Task<BackupFile?> FileByPath(string? filePath)
        {
            return await this.dbContext.Files.FirstOrDefaultAsync(f => f.Path== filePath);
        }

        public async Task<bool> FileExists(string filePath)
        {
            var file = await this.dbContext.Files.FirstOrDefaultAsync(f => f.Path == filePath);
            return file != null;
        }

        public async Task<IEnumerable<BackupFile>> GetBackedUpNull(int batchSize)
        {
            return await this.dbContext.Files.Where(file=>(file.FileBackedUp == 0 || file.FileBackedUp == null)).Take(batchSize).ToListAsync();
        }

        public async Task<BackupFile?> GetFileById(int id)
        {
            return await this.dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<BackupFile>> GetTotalFiles()
        {
            var list = await this.dbContext.Files.ToListAsync();
            return list;
        }

        public async Task<bool> InsertFile(BackupFile file)
        {
            await this.dbContext.AddAsync(file);
            await this.dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<BackupFile>> ListBackedUp()
        {
            return await this.dbContext.Files.Where(file => file.FileBackedUp == 1).ToListAsync();
        }

        public async Task MarkAsBackedUp(int id)
        {
            var file = await this.dbContext.Files.FirstOrDefaultAsync(file => file.Id == id);
            if (file != null) { 
                file.FileBackedUp = 1;
                await this.dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAsDeleted(string filePath)
        {
            var file = await this.dbContext.Files.FirstOrDefaultAsync(file => file.Path == filePath);
            if (file != null)
            {
                file.DeletedAt = DateTime.UtcNow.ToString("O");
                await this.dbContext.SaveChangesAsync();
            }
        }

        public async Task MarkAsFailed(int id, int forcedCount = -1)
        {
            var file = await this.dbContext.Files.FirstOrDefaultAsync(f => f.Id == id);
            if (file != null)
            {
                int failedCount = forcedCount >= 0 ? forcedCount : ((file.FailedAttempts ?? 0) + 1);
                file.FailedAttempts = failedCount;
                file.HistoricFailedAttempts = (file.HistoricFailedAttempts ?? 0) + 1;

                if (failedCount >= 2)
                {
                    file.FileBackedUp = 1;
                }
                await this.dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception("File not found to mark as failed.");
            }
        }

        public async Task OnFileModified(string filePath, string updatedAt)
        {
            var file = await this.dbContext.Files.FirstOrDefaultAsync(f => f.Path == filePath);
            if (file != null)
            {
                file.UpdatedAt = updatedAt;
                file.FileBackedUp = 0;
                file.FailedAttempts = 0;
                file.HistoricFailedAttempts = 0;
                await this.dbContext.SaveChangesAsync();
            }
        }

        public Task<int> PendingFilesCount()
        {
            var count = this.dbContext.Files.CountAsync(f => f.FileBackedUp == 0 || f.FileBackedUp == null);
            return count;
        }

        public async Task ResetFailed()
        {
            var files = await this.dbContext.Files.Where(f => f.FailedAttempts > 0 && f.HistoricFailedAttempts < 18).ToListAsync(); // Solo resetear las que no han fallado más de 18 veces de forma historica.
            files.ForEach(file =>
            {
                file.FailedAttempts = 0;
                file.FileBackedUp = 0;
            });
            await dbContext.BulkUpdateAsync(files);
        }

        public async Task<int> TotalScannedCount()
        {
            return await this.dbContext.Files.CountAsync(x=>x.FileBackedUp != 1);
        }
    }
}
