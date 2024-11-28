using Dapper;
using JpCommon;
using JupiterNeoServiceClient.classes;
using JupiterNeoServiceClient.Utils;
using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic;
using SqlKata;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace JupiterNeoServiceClient.Models
{
    public class BaseModel
    {
        public static SqliteConnection? connection;
        protected QueryFactory? Db;
        
        public Dictionary<Enum, string>? fields;
        protected string TableName { get; set; } = String.Empty;

        protected SqliteCompiler compiler = new SqliteCompiler();

        private static readonly object connectionLock = new(); // Thread-safety lock


        public BaseModel()
        {
            try
            {
                this.SetDatabaseConnection(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Logger.Log(ex, "Failed to set the database (Constructor with new connection)");
            }
        }

        public BaseModel(bool initializeDatabase)
        {
            if (initializeDatabase)
            {
                try
                {
                    this.SetDatabaseConnection(true);
                    if (connection == null || Db == null)
                    {
                        Logger.Log(NULL, "connection or Db is null after SetDatabaseConnection in parameterized constructor.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, "--V8LV--");
                }
            }
        }

        public void SetDatabaseConnection(bool useNewName)
        {
            SQLitePCL.Batteries_V2.Init();
            FileManager fm = new FileManager();
            string dbPath = Path.Combine(fm.AppContainer, JpConstants.SQLiteNameNeo);

            if (!File.Exists(dbPath))
            {
                throw new FileNotFoundException("El archivo de base de datos no se ha encontrado. [SetDatabaseConnection]");
            }

            string source = $"Data Source={dbPath}";
            connection = new SqliteConnection(source);
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Logger.Log(null, "Connection failed to open.");
            }
            Db = new QueryFactory(connection, compiler);
            if (Db == null)
            {
                Logger.Log(null, "QueryFactory initialization failed.");
            }
        }

        protected Query query()
        {
            if (Db == null)
            {
                throw new InvalidOperationException("La conexión a la base de datos no está inicializada.");
            }

            if (string.IsNullOrEmpty(TableName))
            {
                throw new InvalidOperationException("El nombre de la tabla no está definido.");
            }

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

        public static void Disconnect()
        {
            try
            {
                connection?.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "Error al desconectar la base de datos.");
            }
        }
    }
}