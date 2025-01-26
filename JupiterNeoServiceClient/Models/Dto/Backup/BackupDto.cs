using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Models.Dto.Backup
{
    internal interface BackupDto
    {
        public int Id { get; set; }
        public int BackupId { get; set; }
        public int IsStarted { get; set; }
        public int IsCompleted { get; set; }
        public int IsScanned { get; set; }
        public string CreatedAt { get; set; }
    }
}
