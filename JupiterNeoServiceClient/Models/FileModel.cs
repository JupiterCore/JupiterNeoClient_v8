using JupiterNeoServiceClient.classes;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using static JupiterNeoServiceClient.Models.MetadataModel;

namespace JupiterNeoServiceClient.Models
{

    public class FModelNoId
    {
        public string? file_path { get; set; }
        public string? file_added_at { get; set; }
        public string? file_created_at { get; set; }
        public string? file_updated_at { get; set; }
        public string? file_deleted_at { get; set; }
        public string? file_backed_up { get; set; }
        public int file_failed_attempts { get; set; }
    }
    public class FModel : FModelNoId
    {
        public int file_id { get; set; }
    }

    public class FileModel : BaseModel
    {
        public enum FD { ID, PATH, ADDED_AT, CREATED_AT, UPDATED_AT, DELETED_AT, BACKED_UP, FAILED_ATTEMPTS }

        public FileModel()
        {
            this.TableName = "file";

            fields = new Dictionary<Enum, string>();
            fields[FD.ID] = "file_id";
            fields[FD.PATH] = "file_path";
            fields[FD.ADDED_AT] = "file_added_at";
            fields[FD.CREATED_AT] = "file_created_at";
            fields[FD.UPDATED_AT] = "file_updated_at";
            fields[FD.DELETED_AT] = "file_deleted_at";
            fields[FD.BACKED_UP] = "file_backed_up";
            fields[FD.FAILED_ATTEMPTS] = "file_failed_attempts";
        }

        public FModel? fileByPath(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath) || fields == null)
            {
                // Handle the case where filePath is invalid or null.
                Console.WriteLine("Invalid file path provided.");
                return null;
            }

            var result = this.query()
                            .Where(fields[FD.PATH], filePath)
                            .Get<FModel>()
                            .FirstOrDefault();

            return result; // This could still be null if no match is found.
        }

        public bool fileExists(string filePath)
        {
            return this.fileByPath(filePath) != null;
        }


        public bool insertFile(string filePath, string createdAt, string updatedAt)
        {
            FModelNoId m = new FModelNoId();
            m.file_path = filePath;
            m.file_added_at = Helpers.today();
            m.file_created_at = createdAt;
            m.file_updated_at = updatedAt;
            return this.insert(m) > 0;
        }

        public int onFileModified(string filePath, string updatedAt)
        {
            FModel? file = this.fileByPath(filePath);
            if (file != null && fields != null)
            {

                file.file_updated_at = updatedAt;
                file.file_backed_up = null;
                file.file_deleted_at = null;
                return this.Execute(this.query().Where(fields[FD.ID], file.file_id).AsUpdate(file));
            }
            else
            {
                throw new Exception("[onFileModified] is null");
            }
        }

        public IEnumerable<FModel> getBackedUpNull(int batchSize)
        {
            if (fields == null)
            {
                throw new Exception("[getBackedUpNull] cannot be null");
            }
            return this.query().WhereNull(fields[FD.BACKED_UP]).Limit(batchSize).Get<FModel>();
        }

        public IEnumerable<FModel> getTotalFiles()
        {
            return this.query().Get<FModel>();
        }

        public void markAsBackedUp(int id)
        {
            if (fields == null)
            {
                throw new Exception("[getBackedUpNull] cannot be null");
            }
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
            data.Add(fields[FD.BACKED_UP], 1);
            var query = this.query().Where(fields[FD.ID], id).AsUpdate(data);
            this.Execute(query);
        }

        public FModel? getFileById(int id)
        {
            if (fields == null)
            {
                throw new Exception("[getBackedUpNull] cannot be null");
            }
            return this.query().Where(fields[FD.ID], id).Get<FModel>().FirstOrDefault<FModel>();
        }

        public void markAsFailed(int id, int forcedCount = -1)
        {

            FModel? file = this.getFileById(id);
            if (file != null && fields != null)
            {
                int failedCount = forcedCount >= 0 ? forcedCount : (file.file_failed_attempts + 1);
                Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();
                data.Add(fields[FD.FAILED_ATTEMPTS], failedCount);

                if (failedCount >= 3)
                {
                    // Si se ha intentado hacer el backup del archivo y ha fallado por 3 veces se debe marcar como backed up para evitar que esto detenga el resto del backup.
                    data.Add(fields[FD.BACKED_UP], 1);
                }

                var query = this.query().Where(fields[FD.ID], id).AsUpdate(data);
                this.Execute(query);
            }
            else
            {
                throw new Exception("No se ha encontrado el archivo que se intent marcar como fallido.");
            }

        }


        public IEnumerable<FModel> listBackedup()
        {
            if (fields == null)
            {
                throw new Exception("[getBackedUpNull] cannot be null");
            }
            return this.query()
                .Where(fields[FD.BACKED_UP], 1)
                .WhereNull(fields[FD.DELETED_AT]) // No mostrar los archivos borrados
                .Get<FModel>();
        }

        public void deleteStartsWithPathAndHastBackedUp(string startsWith)
        {
            if (fields == null)
            {
                throw new Exception("[getBackedUpNull] cannot be null");
            }
            this.query()
                .WhereStarts(fields[FD.PATH], startsWith)
                .Delete();
        }

        public int markAsDeleted(string filePath)
        {

            FModel? file = this.fileByPath(filePath);
            if (file == null || fields == null) { 
                throw new Exception("[markAsDeleted] cannot be null");
            }
            file.file_deleted_at = Helpers.today();
            return this.Execute(this.query().Where(fields[FD.ID], file.file_id).AsUpdate(file));
        }

        public void resetFailed()
        {
            if (fields == null) {
                throw new Exception("[resetFailed] Fields cannot be null");
            }
            var updateData = new Dictionary<string, dynamic>();
            updateData.Add(fields[FD.BACKED_UP], "");
            updateData.Add(fields[FD.FAILED_ATTEMPTS], 0);
            this.Execute(this.query().Where(fields[FD.FAILED_ATTEMPTS], ">", 0).AsUpdate(updateData));
        }


        public int pendingFilesCount()
        {
            if (fields == null)
            {
                throw new Exception("[pendingFilesCount] cannot be null");
            }
            return this.query()
                .WhereNull(fields[FD.BACKED_UP])
                .AsCount().FirstOrDefault<int>();
        }

        public int totalScannedCount()
        {
            return this.query()
            .AsCount().FirstOrDefault<int>();
        }
    }
}
