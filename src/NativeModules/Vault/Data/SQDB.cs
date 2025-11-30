using Essentials.Core;
using Essentials.NativeModules.Vault.Enums;
using System;
using System.IO;

namespace Essentials.NativeModules.Vault.data
{
    internal static class DatabaseManager
    {
        private static readonly string LiteDB_FileName = "vault.db";
        private static string LiteDB_FilePath;
        internal static string LiteDB_ConnectionString;

        internal static string MySql_TableName;
        internal static string MySql_ConnectionString;

        internal static SerialQueue Queue;
        internal static void Initialize()
        {
            var DataPath = Path.Combine(EssCore.Instance.Directory, "data");
            var config = EssCore.Instance.Config.Vaultconfig;

            // Validar que la carpeta exista
            if (!Directory.Exists(DataPath))
            {
                Console.WriteLine($"[Vault] La carpeta de datos no existe: {DataPath}. Asegúrate de que otra parte del plugin la cree.");
            }

            LiteDB_FilePath = Path.Combine(DataPath, LiteDB_FileName);
            LiteDB_ConnectionString = $"Filename={LiteDB_FilePath};Connection=shared;";

            // Validar que el connectionString no sea null
            if (string.IsNullOrWhiteSpace(LiteDB_ConnectionString))
                throw new InvalidOperationException("LiteDB connection string es null. Asegúrate de llamar a DatabaseManager.Initialize() correctamente.");

            Queue = new SerialQueue();

            // Configuración MySQL
            if (config.Database == EDatabase.MYSQL)
            {
                if (string.IsNullOrWhiteSpace(config.MySqlConnectionString))
                    throw new InvalidOperationException("MySQL connection string is null o vacía.");

                var index = config.MySqlConnectionString.LastIndexOf("TABLENAME", StringComparison.Ordinal);
                if (index == -1)
                {
                    MySql_TableName = "Essentials";
                    MySql_ConnectionString = config.MySqlConnectionString;
                }
                else
                {
                    var substr = config.MySqlConnectionString.Substring(config.MySqlConnectionString.LastIndexOf('='));
                    MySql_TableName = substr.Substring(1, substr.Length - 1);
                    MySql_ConnectionString = config.MySqlConnectionString.Remove(index);
                }
            }
        }
    }
}

