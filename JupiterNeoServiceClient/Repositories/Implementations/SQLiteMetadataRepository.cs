using JupiterNeoServiceClient.Data;
using JupiterNeoServiceClient.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JupiterNeoServiceClient.Repositories.Implementations
{
    internal class SQLiteMetadataRepository : ImetadataRepository
    {
        private readonly NeoDbContext dbContext;

        public SQLiteMetadataRepository(NeoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<string?> GetLicense()
        {

            var all = await dbContext.Metadatas.ToListAsync();
            var result = await dbContext.Metadatas.FirstOrDefaultAsync(x => x.Type == "license");
            if (result == null)
            {
                return null;
            }
            return result.Data;
        }
    }
}
