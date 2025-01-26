using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Models.Dto.File
{
    internal class FileDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string AddedAt { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string DeletedAt { get; set; }
        public int FileBackedUp { get; set; }
        public int FailedAttempts { get; set; }
        public int HistoricFailedAttempts { get; set; }
    }
}
