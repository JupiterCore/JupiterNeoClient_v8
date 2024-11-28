using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JupiterNeoServiceClient.Models
{

    public class MetadataModel : BaseModel
    {

        public class MdModelNoId
        {
            public string meta_type { get; set; }
            public string meta_data { get; set; }
        }
        public class MdModel : MdModelNoId
        {
            public int meta_id { get; set; }

        }

        internal MdModel model = new MdModel();

        public enum fd { ID, TYPE, DATA } // Columnas de la base de datos representadas por un enumerable
        public enum ts { LICENSE, FIRST_BACKUP, BACKUP_ID }

        public Dictionary<Enum, string> Types; // Tipos de valores para el campo meta_type (puede ser cualquier string)

        public MetadataModel()
        {
            this.initialize();
        }

        public MetadataModel(bool initializeDatabase) : base(initializeDatabase)
        {
            /**
             * Its required to not initialize the database when we want to use the old database.
             * */
            this.initialize();
        }

        private void initialize()
        {
            // Establecer el nombre de la tabla para las consultas.
            this.TableName = "metadata";
            // Establecer los nombres de las columnas de la tabla con clave del enum definido arriba y en el valor el nombre de la columna.
            this.fields = new Dictionary<Enum, string>();
            this.fields[fd.ID] = "meta_id";
            this.fields[fd.TYPE] = "meta_type";
            this.fields[fd.DATA] = "meta_data";
            Types = new Dictionary<Enum, string>();
            // Establecer los valores permitidos en el campo meta_type.
            this.Types[ts.LICENSE] = "license";
            this.Types[ts.FIRST_BACKUP] = "first_backup";
            this.Types[ts.BACKUP_ID] = "backup_id";
        }

        // ¿Existe un valor con el tipo (type) en meta_type? Algo como: SELECT * FROM metadata where meta_type = type
        public bool hasMetadata(Enum type)
        {
            string field = this.fields[fd.TYPE];
            string value = this.Types[type];

            var result = this.query().Where(field, value).Get<MdModel>();
            return result.Count() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "license"
        public bool hasLicense()
        {
            return this.hasMetadata(ts.LICENSE);
        }

        public bool hasBackupId()
        {
            return this.hasMetadata(ts.BACKUP_ID);
        }

        public string getBackupId()
        {
            var result = this.query().Where(this.fields[fd.TYPE], this.Types[ts.BACKUP_ID]).Get<MdModel>().FirstOrDefault();
            if (result == null)
            {
                return null;
            }
            else
            {
                return result.meta_data;
            }
        }

        public bool insertBackupId(string backupId)
        {
            return this.insert(backupId, ts.BACKUP_ID);
        }

        public bool deleteBackup()
        {
            return this.query().Where(this.fields[fd.TYPE], this.Types[ts.BACKUP_ID]).Delete() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "first_backup"
        public bool hasFirstBackup()
        {
            return this.hasMetadata(ts.FIRST_BACKUP);
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (type, data);
        public bool insert(string data, Enum type)
        {
            var allData = new MdModelNoId();
            allData.meta_type = this.Types[type];
            allData.meta_data = data;

            var result = this.insert(allData);
            return result > 0;
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (license, data);
        public bool insertLicense(string data)
        {
            return this.insert(data, ts.LICENSE);
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (first_backup, data);
        public bool insertFirstBackup(string data)
        {
            return this.insert(data, ts.FIRST_BACKUP);
        }

        public IEnumerable<MdModel> getType(Enum type)
        {
            return this.query().Where(this.fields[fd.TYPE], this.Types[type]).Get<MdModel>();
        }

        public string getLicense()
        {
            if (this.hasLicense())
            {
                var result = this.getType(ts.LICENSE);
                MdModel first = result.First();
                return first.meta_data;
            }
            else
            {
                return "";
            }
        }

    }

}
