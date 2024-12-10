using JpCommon;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;

namespace JupiterNeoServiceClient.Controllers
{
    public class BaseController
    {
        protected readonly MetadataModel MetaModel;
        protected readonly JpApi Api;
        protected string License { get; private set; } = string.Empty;

        // Constructor
        public BaseController(MetadataModel metaModel, JpApi api)
        {
            MetaModel = metaModel ?? throw new ArgumentNullException(nameof(metaModel));
            Api = api ?? throw new ArgumentNullException(nameof(api));

            try
            {
                License = MetaModel.getLicense();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "--base-controller--");
            }
        }
    }
}
