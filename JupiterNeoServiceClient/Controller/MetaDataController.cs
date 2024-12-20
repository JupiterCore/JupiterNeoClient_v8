using JpCommon;
using JupiterNeoServiceClient.Controllers;
using JupiterNeoServiceClient.Models;
using JupiterNeoServiceClient.Utils;
using System;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Controller
{
    public class MetaDataController : BaseController
    {
        private readonly MetadataModel _model;

        public string BackupId { get; private set; }

        public MetaDataController(MetadataModel model, JpApi api) : base(model, api)  
        {
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
                if (!string.IsNullOrEmpty(this.License) && BackupId == null)
                {
                    var backupId = await Api.GetBackupIdAsync(this.License, pendingFilesCount);

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

        public async Task ConcludeBackupAsync(string bid)
        {
            try
            {
                var response = await Api.ConcludeBackupAsync(this.License, bid);
                response.EnsureSuccessStatusCode();
                _model.deleteBackup();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "--conclude-backup--");
            }
        }
    }
}
