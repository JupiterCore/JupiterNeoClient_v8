using JupiterNeoServiceClient.Data;
using JupiterNeoServiceClient.Models.Domain;
using JupiterNeoServiceClient.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;

namespace JupiterNeoServiceClient.Repositories.Implementations
{
    internal class SQLiteBakcupPathsRepository : IBackupPathRepository
    {
        private readonly NeoDbContext dbContext;

        public SQLiteBakcupPathsRepository(NeoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task AddPath(string path)
        {

            var backupPath = new BackupPaths
            {
                Path = path,
            };

            await this.dbContext.BackupPaths.AddAsync(backupPath);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<BackupPaths?> ByPath(string path)
        {
            return await this.dbContext.BackupPaths.FirstOrDefaultAsync(f => f.Path == path);
        }

        public async Task DeleteByPath(string path)
        {
            var elementsToDelete = await this.dbContext.BackupPaths.Where(paths => (paths.Path == path)).ToListAsync();
            this.dbContext.BackupPaths.RemoveRange(elementsToDelete);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsPath(string path)
        {
            var pathExists = await this.dbContext.BackupPaths.FirstOrDefaultAsync(f => f.Path == path);
            return pathExists != null;
        }

        public async Task<List<BackupPaths>> GetAllPaths()
        {
            return await this.dbContext.BackupPaths.ToListAsync();
        }

    }
}
