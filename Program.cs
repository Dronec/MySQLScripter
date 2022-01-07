using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MySQLScripter
{
    class Program
    {
        private static List<ScriptingObject> ObjReference = new List<ScriptingObject>()
        {
            new ScriptingObject("sp",
                "SELECT ROUTINE_NAME as NAME FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_SCHEMA = @schema;",
                "SHOW CREATE PROCEDURE", "Create Procedure"
            ),
            new ScriptingObject("table",
                "SELECT TABLE_NAME as NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_SCHEMA = @schema;",
                "SHOW CREATE TABLE", "Create Table"
            )
            ,
            new ScriptingObject("view",
                "SELECT TABLE_NAME as NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='VIEW' AND TABLE_SCHEMA = @schema;",
                "SHOW CREATE VIEW", "Create View"
            )
        };
        private static string _path;
        private static string _objectType;
        private static string _database;
        private static string _server;
        private static uint _port;
        private static string _user;
        private static string _password;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Help();
                Environment.Exit(0);
            }
            Dictionary<string, string> arguments = args.Select(x => x.Split("=")).ToDictionary(x => x[0], x => x[1]);

            foreach (KeyValuePair<string, string> a in arguments)
            {
                Console.WriteLine($"{a.Key} : {a.Value}");
            }

            try
            {
                _path = arguments["path"];
                _objectType = arguments["type"];
                _database = arguments["database"];
                _server = arguments["server"];
                _port = Convert.ToUInt32(arguments["port"]);
                _user = arguments["user"];
                _password = arguments["pw"];
            }
            catch
            {
                Help();
                Environment.Exit(0);
            }

            ScriptingObject objectStructure = ObjReference.First(x => x.Type == _objectType);

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = _server,
                UserID = _user,
                Password = _password,
                Database = _database,
                Port = _port
            };

            using MySqlConnection connection = new MySqlConnection(builder.ConnectionString);
            connection.OpenAsync().Wait();

            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = objectStructure.GetList;
            command.Parameters.AddWithValue("@schema", _database);

            using MySqlDataReader reader = command.ExecuteReaderAsync().Result;
            while (reader.Read())
            {
                string name = reader.GetString("NAME");
                Console.WriteLine($"Scripting {_objectType} '{name}...'");
                ScriptObject(objectStructure.GetObject, name, objectStructure.OutputField, builder.ConnectionString);
            }
            Console.WriteLine("Scripting finished.");
            Environment.Exit(0);
        }

        static void ScriptObject(string query, string name, string outputField, string connectionString)
        {
            using MySqlConnection connection = new MySqlConnection(connectionString);
            connection.OpenAsync().Wait();
            using MySqlCommand command = connection.CreateCommand();
            command.CommandText = $"{query} `{name}`;";
            using MySqlDataReader reader = command.ExecuteReaderAsync().Result;
            while (reader.Read())
            {
                string definition = reader.GetString(outputField);
                File.WriteAllText(Path.Combine(_path, $"{name}.txt"), definition);
            }
        }
        static void Help()
        {
            Console.WriteLine(@"MySQL Scripter usage:
MySQLScripter.exe path=[output path] type=[sp or table or view] database=[database name] server=[server name] port=[server port] user=[username] pw=[password]");
        }
    }
}
