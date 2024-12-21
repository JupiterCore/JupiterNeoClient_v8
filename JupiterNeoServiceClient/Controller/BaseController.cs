using JpCommon;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;

namespace JupiterNeoServiceClient.Controllers
{
    public class BaseController
    {

        public MetadataModel metaModel = new MetadataModel();
        public string license = "";
        public JpApi api = new JpApi();

        public BaseController()
        {
            try
            {
                this.license = metaModel.getLicense();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "--base-controller--");
            }
        }
    }
}
