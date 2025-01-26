using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Repositories.Interfaces
{
    internal interface ImetadataRepository
    {
        Task<string?> GetLicense();
    }
}
