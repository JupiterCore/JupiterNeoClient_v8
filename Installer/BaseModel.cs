using Dapper;
using JpCommon;
using Microsoft.Data.Sqlite;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
namespace Installer
{
    public class BaseModel
    {
        protected static SqliteConnection connection;
        protected QueryFactory Db;

        public Dictionary<Enum, string> fields;
        protected string TableName { get; set; }

        protected SqliteCompiler compiler = new SqliteCompiler();

        public BaseModel()
        {

            try
            {
                /*
                SQLitePCL.Batteries.Init();
                string apContainer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JupiterNeoClient");
                string dbPath = Path.Combine(apContainer, JpConstants.SQLiteNameNeo);
                string source = $"Data Source={dbPath}";
                connection = new SqliteConnection(source);
                connection.Open();
                Db = new QueryFactory(connection, compiler);
                */
                this.SetDatabaseConnection(true);
            }
            catch (Exception)
            {
            }
        }

        public BaseModel(bool useNewName)
        {
            try { 
                this.SetDatabaseConnection(useNewName);
            }catch(Exception) { 
                
            }
        }


        public void SetDatabaseConnection(bool useNewName)
        {

            SQLitePCL.Batteries.Init();
            string apContainer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JupiterNeoClient");
            string dbPath = Path.Combine(apContainer, useNewName ? JpConstants.SQLiteNameNeo : "database.sqlite");
            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("El archivo de base de datos no se ha encontrado. [SetDatabaseConnection]");
            }
            string source = $"Data Source={dbPath}";
            connection = new SqliteConnection(source);
            connection.Open();
            Db = new QueryFactory(connection, compiler);
        }

        protected SqlKata.Query query()
        {
            return this.Db.Query(TableName);
        }

        protected int insert(object obj)
        {
            Query query = this.query().AsInsert(obj);
            SqlResult compiledQuery = compiler.Compile(query);
            return connection.Execute(compiledQuery.Sql, compiledQuery.NamedBindings);
        }

        protected int update(object obj)
        {
            Query query = this.query().AsUpdate(obj);
            SqlResult compiledQuery = compiler.Compile(query);
            return connection.Execute(compiledQuery.Sql, compiledQuery.NamedBindings);
        }

        protected int Execute(Query query)
        {
            SqlResult compiledQuery = compiler.Compile(query);
            return connection.Execute(compiledQuery.Sql, compiledQuery.NamedBindings);
        }
    }
}
