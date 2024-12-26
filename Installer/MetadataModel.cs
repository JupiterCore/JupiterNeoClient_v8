using SqlKata.Execution;

namespace Installer
{
    public class MetadataModel : BaseModel
    {
        public class MdModelNoId
        {
            public string meta_type { get; set; }
            public string meta_data { get; set; }

            public MdModelNoId()
            {
                this.meta_type = "";
                this.meta_data = "";
            }

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
        public MetadataModel(bool useNewName) : base(useNewName)
        {
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
            if (this.Types == null)
            {
                throw new Exception("[hasMetadata] Types is null");
            }
            string field = this.fields[FD.TYPE];
            string value = this.Types[type];

            var result = this.Db.Query(this.TableName).Where(field, value).Get<MdModel>();

            //var result = this.query().Where(field, value).Get<MdModel>();
            return result.Count() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "license"
        public bool hasLicense()
        {
            return this.hasMetadata(TS.LICENSE);
        }


        // INSERT INTO metadata (meta_type, meta_data) VALUES (type, data);
        public bool insert(string data, Enum type)
        {
            if (this.Types == null)
            {
                throw new Exception("[insert] Types is null");
            }
            // Dictionary<string, string> allData = new Dictionary<string, string>();
            // allData.Add(this.fields[fd.TYPE], this.Types[type]);
            // allData.Add(this.fields[fd.DATA], data);
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

        public IEnumerable<MdModel> getType(Enum type)
        {
            if (this.Types == null)
            {
                throw new Exception("[getType] Types is null");
            }
            return this.query().Where(this.fields[FD.TYPE], this.Types[type]).Get<MdModel>();
        }

        public string getLicense()
        {
            if (this.hasLicense())
            {
                var result = this.getType(TS.LICENSE);
                MdModel first = result.First();
                return first.meta_data;
            }
            else
            {
                return "";
            }
        }

        public bool removeLicense()
        {
            if (this.Types == null)
            {
                throw new Exception("[removeLicense] Types is null");
            }
            return this.query().Where(fields[FD.TYPE], this.Types[TS.LICENSE]).Delete() > 0;
        }

    }
}