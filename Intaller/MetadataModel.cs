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
        public enum fd { ID, TYPE, DATA } // Columnas de la base de datos representadas por un enumerable
        public enum ts { LICENSE, FIRST_BACKUP, BACKUP_ID }

        public Dictionary<Enum, string> Types; // Tipos de valores para el campo meta_type (puede ser cualquier string)

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

            var result = this.Db.Query(this.TableName).Where(field, value).Get<MdModel>();

            //var result = this.query().Where(field, value).Get<MdModel>();
            return result.Count() > 0;
        }

        // SELECT * FROM meta_type where meta_type = "license"
        public bool hasLicense()
        {
            return this.hasMetadata(ts.LICENSE);
        }


        // INSERT INTO metadata (meta_type, meta_data) VALUES (type, data);
        public bool insert(string data, Enum type)
        {

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
            return this.insert(data, ts.LICENSE);
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

        public bool removeLicense()
        {
            return this.query().Where(fields[fd.TYPE], this.Types[ts.LICENSE]).Delete() > 0;
        }

    }
}

