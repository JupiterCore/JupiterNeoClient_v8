using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JpCommon
{
    public class JpConstants
    {
        // public static readonly string UpdaterServiceName = "JupiterNeoUpdaterService";
        public static readonly string ClientServiceName = "JupiterNeoClientService";
        public static readonly string ClientFolderName = "JupiterNeoClient";
        public static readonly string ClientExeName = "JupiterNeoServiceClient.exe";

        public static readonly string UpdaterFolderName = "JupiterNeoUpdater";
        public static readonly string UpdaterExeName = "JupiterNeoUpdateService.exe";
        public static readonly string UpdaterServiceName = "JupiterNeoUpdaterService";

#if DEBUG
        public static readonly string ApiBaseUrl = "http://localhost:8080/krn/v1";
        public static readonly string UpdaterBaseUrl = "http://localhost:3200";
#else
        public static readonly string ApiBaseUrl = "https://api.jupiterneo.cloud/krn/v1";
        public static readonly string UpdaterBaseUrl = "https://update.jupiterneo.cloud";
#endif

        public static readonly string SQLiteNameNeo = "db1-0-0.sqlite";
    }
}
