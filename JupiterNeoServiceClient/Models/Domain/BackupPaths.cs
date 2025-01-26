using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Models.Domain
{

    [Table("backup_paths")]
    public class BackupPaths
    {

        [Column("bapa_id")]
        public int Id { get; set; }
        [Column("bapa_path")]
        public required string Path { get; set; }

    }
}
