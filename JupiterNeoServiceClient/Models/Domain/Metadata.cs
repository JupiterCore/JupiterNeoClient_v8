using System.ComponentModel.DataAnnotations.Schema;

namespace JupiterNeoServiceClient.Models.Domain
{
    [Table("metadata")]
    internal class Metadata
    {
        [Column("meta_id")]
        public int Id { get; set; }

        [Column("meta_type")]
        public string Type { get; set; }

        [Column("meta_data")]
        public string Data { get; set; }
    }

}
