
using JupiterNeoServiceClient.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace JupiterNeoServiceClient.Data
{
    public class NeoDbContext : DbContext
    {

        internal DbSet<BackupPaths> BackupPaths { get; set; }
        internal DbSet<BackupFile> Files { get; set; }
        internal DbSet<Backup> Backups { get; set; }
        internal DbSet<Metadata> Metadatas { get; set; }

        public NeoDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {
        }

    }
}
