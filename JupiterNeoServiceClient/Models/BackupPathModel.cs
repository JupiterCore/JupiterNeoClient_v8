using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JupiterNeoServiceClient.Models
{
    public class BackupPathNoId
    {
        public string? bapa_path { get; set; }

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
        public FullModel()
        {

        }
        public FullModel(string path) : base(path)
        {
        }

        public int bapa_id { get; set; }
    }

    public class BackupPathModel : BaseModel
    {
        public enum FD { ID, PATH }
        public BackupPathModel()
        {
            this.TableName = "backup_paths";
            this.fields = new Dictionary<Enum, string>();
            this.fields[FD.ID] = "bapa_id";
            this.fields[FD.PATH] = "bapa_path";
        }

        public FullModel? byPath(string path)
        {
            if (fields == null)
            {
                throw new Exception("[byPath] cannot be null");
            }
            return this.query().Where(fields[FD.PATH], path).Get<FullModel>().FirstOrDefault();
        }

        public bool existsPath(string path)
        {
            return this.byPath(path) != null;
        }

        public List<FullModel> getAllPaths()
        {
            return this.query().Get<FullModel>().ToList();
        }

        public void deleteByPath(string path)
        {
            if (fields == null)
            {
                throw new Exception("[deleteByPath] cannot be null");
            }
            this.query().Where(this.fields[FD.PATH], path).Delete();
        }

        public void addPath(string path)
        {
            var insertData = new FullModel(path);
            this.query().Insert(insertData);
        }
    }
}
