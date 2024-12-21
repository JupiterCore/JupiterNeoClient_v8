using JupiterNeoServiceClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Controllers
{
    public class BackupPathController
    {
        private BackupPathModel backupPathModel { get; set; }
        public BackupPathController()
        {
            this.backupPathModel = new BackupPathModel();
        }

        public void updatePaths(string[] newPaths)
        {

            var currentPaths = this.backupPathModel.getAllPaths();
            List<string> currentPathsList = new List<string>();
            foreach (var path in currentPaths)
            {
                if (path.bapa_path != null)
                {
                    currentPathsList.Add(path.bapa_path);
                }
            }

            /*
            * New paths are all the paths that the online system has set.
            * the current paths are any paths that were previously set in the local db (could be outdated)
            */
            foreach (string path in newPaths)
            {
                // If the path in the remote db is not present in the local db we must add it locally.
                bool existsLocally = this.backupPathModel.existsPath(path);
                if (!existsLocally)
                {
                    this.backupPathModel.addPath(path);
                }
            }

            /**
             * The user could have removed from paths remotely but locally a previous version could have added them.
             * We need to remove old records that no longer exist.
             **/
            foreach (string path in currentPathsList)
            {
                // the new paths does not contain this current path
                if (!newPaths.Contains(path))
                {
                    this.backupPathModel.deleteByPath(path);
                }
            }
        }

    }
}

