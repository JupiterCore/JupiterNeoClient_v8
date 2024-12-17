using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{
    public class BackupPathNoId
    {
        public string bapa_path { get; set; } = string.Empty;

        public BackupPathNoId()
        {
        }

        public BackupPathNoId(string path)
        {
            this.bapa_path = path;
        }
    }

    public class FullModel : BackupPathNoId
    {
        public int bapa_id { get; set; }

        public FullModel()
        {
        }

        public FullModel(string path) : base(path)
        {
        }
    }

    public class BackupPathModel : BaseModel
    {
        public enum fd { ID, PATH }

        public BackupPathModel()
        {
            this.TableName = "backup_paths";
            this.fields = new Dictionary<Enum, string>
            {
                [fd.ID] = "bapa_id",
                [fd.PATH] = "bapa_path"
            };
        }

        public FullModel? byPath(string path)
        {
            return this.query()
                       .Where(fields[fd.PATH], path)
                       .Get<FullModel>()
                       .FirstOrDefault();
        }

        public bool ExistsPath(string path)
        {
            return this.byPath(path) != null;
        }

        public List<FullModel> GetAllPaths()
        {
            return this.query().Get<FullModel>().ToList();
        }

        public void DeleteByPath(string path)
        {
            this.query()
                .Where(fields[fd.PATH], path)
                .Delete();
        }

        public void AddPath(string path)
        {
            var insertData = new FullModel(path);
            this.query().Insert(insertData);
        }
    }
}
