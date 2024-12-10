using JupiterNeoServiceClient.Utils;
using JupiterNeoServiceClient.Models;
using JpCommon;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Controllers
{
    public class ProgramsController : BaseController
    {
        private readonly ProgramListFetcher _programListFetcher;

        public ProgramsController(
            ProgramListFetcher programListFetcher,
            MetadataModel metaModel,
            JpApi api
        ) : base(metaModel, api)
        {
            _programListFetcher = programListFetcher ?? throw new ArgumentNullException(nameof(programListFetcher));
        }

        public async Task<bool> NotifyProgramsAsync()
        {
            if (string.IsNullOrEmpty(License))
            {
                throw new InvalidOperationException("License is not set.");
            }

            try
            {
                // Obtener lista de programas instalados
                List<string> installedPrograms = _programListFetcher.GetInstalledProgramsList();

                // Enviar la lista a través de la API
                var response = await Api.NotifyInstalledProgramsAsync(License, installedPrograms);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error notifying programs in ProgramsController");
                return false;
            }
        }
    }
}
