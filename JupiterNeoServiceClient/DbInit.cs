using JupiterNeoServiceClient.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient
{
    public class DbInit
    {
        private readonly NeoDbContext dbContext;

        public DbInit(NeoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void EnsureTableCreated()
        {
            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Backups (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    BackupId INTEGER UNIQUE NOT NULL,
                    IsStarted INTEGER NOT NULL CHECK(IsStarted IN (0, 1)),
                    IsCompleted INTEGER NOT NULL CHECK(IsCompleted IN (0, 1)),
                    IsScanned INTEGER NOT NULL CHECK(IsScanned IN (0, 1)),
                    CreatedAt TEXT NOT NULL
                );
            ";
            dbContext.Database.ExecuteSqlRaw(createTableQuery);




            /***
             * 
             * Verificar si la columna file_historic_failed_attempts existe o si no crearla.
             * 
             */
            var columnExistsQuery = @"
                SELECT 1 
                FROM pragma_table_info('file') 
                WHERE name = 'file_historic_failed_attempts';
            ";

            // Execute the query and check if the column exists
            using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = columnExistsQuery;
                dbContext.Database.OpenConnection();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read()) // Column does not exist
                    {
                        var addColumnQuery = @"ALTER TABLE file ADD COLUMN file_historic_failed_attempts INTEGER DEFAULT 0;";
                        dbContext.Database.ExecuteSqlRaw(addColumnQuery);
                    }
                }
            }

        }
    }
}
