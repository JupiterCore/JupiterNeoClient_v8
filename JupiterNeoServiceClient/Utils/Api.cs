using JpCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Utils
{
    public class BackupSchedule
    {
        public string[] paths { get; set; } = new string[0];
        public string[] schedules { get; set; } = new string[0];
    }

    public class Api : BaseHttp
    {
        public Api()
        {
            // this.baseURL = "http://localhost:8080/krn/v1";
            // this.baseURL = "https://api.jupiterneo.cloud/krn/v1";
            this.baseURL = JpConstants.ApiBaseUrl;
        }
    }
}
