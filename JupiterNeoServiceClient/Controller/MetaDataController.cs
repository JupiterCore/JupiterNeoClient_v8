using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Controller
{
    public class MetaDataController : BaseController
    {
        private readonly Api _api;
        private readonly MetadataModel _model;

        public string BackupId { get; private set; }

        public MetaDataController(Api api, MetadataModel model)
        {
            _api = api;
            _model = model;
        }

        public string GetBackupId()
        {
            BackupId = null;

            try
            {
                BackupId = _model.getBackupId();
            }
            catch (Exception ex)
            {
                BackupId = null;
                Logger.Log(ex, "--meta-1--");
            }

            return BackupId;
        }

        public async Task RequestBackup(int pendingFilesCount)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.license) && BackupId == null)
                {
                    var backupId = await _api.getBackupId(this.license, pendingFilesCount);

                    if (!string.IsNullOrEmpty(backupId))
                    {
                        _model.deleteBackup(); // Delete any possible remaining backup record
                        _model.insertBackupId(backupId); // Add the newest record.
                    }
                }
            }
            catch (Exception ex)
            {
                BackupId = null;
                Logger.Log(ex, "--request--");
            }
        }

        public async Task ConcludeBackup(string bid)
        {
            var response = await _api.concludeBackup(this.license, bid);
            response.EnsureSuccessStatusCode();
            _model.deleteBackup();
        }
    }
}
