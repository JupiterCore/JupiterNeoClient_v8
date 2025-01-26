using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace JupiterNeoServiceClient.Models.Domain
{
    internal class Backup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Index(IsUnique = true)] // EF Core does not have direct support for [Index] yet; use Fluent API for this.
        public int BackupId { get; set; }

        [Required]
        public int IsStarted { get; set; } // 0 or 1

        [Required]
        public int IsCompleted { get; set; } // 0 or 1

        [Required]
        public int IsScanned { get; set; } // 0 or 1

        [Required]
        public string CreatedAt { get; set; } // ISO 8601 formatted string (e.g., "2025-01-01T12:00:00Z")
    }
}
