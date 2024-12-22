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
            public string? meta_type { get; set; }
            public string? meta_data { get; set; }
        }
        public class MdModel : MdModelNoId
        {
            public int meta_id { get; set; }

        }

        internal MdModel model = new MdModel();

        public enum FD { ID, TYPE, DATA } // Columnas de la base de datos representadas por un enumerable
        public enum TS { LICENSE, FIRST_BACKUP, BACKUP_ID }

        public Dictionary<Enum, string>? Types; // Tipos de valores para el campo meta_type (puede ser cualquier string)

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
            this.fields[FD.ID] = "meta_id";
            this.fields[FD.TYPE] = "meta_type";
            this.fields[FD.DATA] = "meta_data";
            Types = new Dictionary<Enum, string>();
            // Establecer los valores permitidos en el campo meta_type.
            this.Types[TS.LICENSE] = "license";
            this.Types[TS.FIRST_BACKUP] = "first_backup";
            this.Types[TS.BACKUP_ID] = "backup_id";
        }

        // ¿Existe un valor con el tipo (type) en meta_type? Algo como: SELECT * FROM metadata where meta_type = type
        public bool hasMetadata(Enum type)
        {

            if (this.fields == null || this.Types == null)
            {
                throw new Exception("[hasMetadata] fields and Types cant be empty");
            }

            string field = this.fields[FD.TYPE];
            string value = this.Types[type];

            var result = this.query().Where(field, value).Get<MdModel>();
            return result.Count() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "license"
        public bool hasLicense()
        {
            return this.hasMetadata(TS.LICENSE);
        }

        public bool hasBackupId()
        {
            return this.hasMetadata(TS.BACKUP_ID);
        }

        public string? getBackupId()
        {

            if (this.fields == null || this.Types == null)
            {
                throw new Exception("[getBackupId] fields and Types cant be null");
            }
            var result = this.query().Where(this.fields[FD.TYPE], this.Types[TS.BACKUP_ID]).Get<MdModel>().FirstOrDefault();
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
            return this.insert(backupId, TS.BACKUP_ID);
        }

        public bool deleteBackup()
        {
            if (this.fields == null || this.Types == null)
            {
                throw new Exception("[deleteBackup] fields and types cant be null");
            }
            return this.query().Where(this.fields[FD.TYPE], this.Types[TS.BACKUP_ID]).Delete() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "first_backup"
        public bool hasFirstBackup()
        {
            return this.hasMetadata(TS.FIRST_BACKUP);
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (type, data);
        public bool insert(string data, Enum type)
        {

            if (this.Types == null)
            {
                throw new Exception("[insert] Types cant be null");
            }

            var allData = new MdModelNoId();
            allData.meta_type = this.Types[type];
            allData.meta_data = data;

            var result = this.insert(allData);
            return result > 0;
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (license, data);
        public bool insertLicense(string data)
        {
            return this.insert(data, TS.LICENSE);
        }

        // INSERT INTO metadata (meta_type, meta_data) VALUES (first_backup, data);
        public bool insertFirstBackup(string data)
        {
            return this.insert(data, TS.FIRST_BACKUP);
        }

        public IEnumerable<MdModel> getType(Enum type)
        {
            if (this.Types == null || this.fields == null)
            {
                throw new Exception("[getType] Types and fields cant be null");
            }
            return this.query().Where(this.fields[FD.TYPE], this.Types[type]).Get<MdModel>();
        }

        public string getLicense()
        {
            if (this.hasLicense())
            {
                var result = this.getType(TS.LICENSE);
                MdModel first = result.First();
                if (first == null || first.meta_data == null)
                {
                    return "";
                }
                return first.meta_data;
            }
            else
            {
                return "";
            }
        }

    }

}
