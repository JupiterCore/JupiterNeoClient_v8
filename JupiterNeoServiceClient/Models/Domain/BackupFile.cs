using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Models.Domain
{

    [Table("file")]
    public class BackupFile
    {
        [Column("file_id")]
        public int Id { get; set; }

        [Column("file_path")]
        public string? Path { get; set; }

        [Column("file_added_at")]
        public string? AddedAt { get; set; }


        [Column("file_created_at")]
        public string? CreatedAt { get; set; }

        [Column("file_updated_at")]
        public string? UpdatedAt { get; set; }

        [Column("file_deleted_at")]
        public string? DeletedAt { get; set; }

        [Column("file_backed_up")]
        public int? FileBackedUp { get; set; }

        [Column("file_failed_attempts")]
        public int? FailedAttempts { get; set; }

        [Column("file_historic_failed_attempts")]
        public int? HistoricFailedAttempts { get; set; }
    }
}
