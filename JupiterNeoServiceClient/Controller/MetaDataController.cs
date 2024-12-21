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

        public MetadataModel model = new MetadataModel();
        public string? backupId { get; set; }

        public string? getBackupId()
        {
            this.backupId = null;
            try
            {
                this.backupId = model.getBackupId();
            }
            catch (Exception ex)
            {
                backupId = null;
                Logger.Log(ex, "--meta-1--");
            }
            return backupId;
        }

        public async Task requestBackup(int pendingFilesCount)
        {

            try
            {
                if (this.license != null && backupId == null)
                {
                    var backupId = await this.api.getBackupId(this.license, pendingFilesCount);

                    if (backupId != null && backupId.Length > 0)
                    {
                        this.model.deleteBackup(); // Delete any possible remaining backup record
                        this.model.insertBackupId(backupId); // Add the newest record.
                    }
                }
            }
            catch (Exception ex)
            {
                backupId = null;
                Logger.Log(ex, "--request--");
            }
        }

        public async Task concludeBackup(string bid)
        {

            var response = await this.api.concludeBackup(this.license, bid);
            response.EnsureSuccessStatusCode();
            this.model.deleteBackup();
        }

    }

}
