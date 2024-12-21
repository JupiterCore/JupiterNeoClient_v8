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

        public async Task<bool> notifyPrograms()
        {

            bool wasNotified = false;
            if (this.license != null)
            {
                try
                {
                    ProgramListFetcher programListFetcher = new ProgramListFetcher();
                    List<string> programs = programListFetcher.GetInstalledProgramsList();
                    var response = await this.api.NotifyInstalledPrograms(this.license, programs);
                    response.EnsureSuccessStatusCode();
                    wasNotified = true;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, "---vvvv-pe-01--");
                }
            }
            return wasNotified;
        }

    }
}
