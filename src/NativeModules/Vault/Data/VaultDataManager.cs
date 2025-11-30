using Cysharp.Threading.Tasks;
using Dapper;
using Essentials.Api;
using Essentials.Core;
using Essentials.NativeModules.Vault.Enums;
using Essentials.NativeModules.Vault.Models;
using Essentials.NativeModules.Vault.Utils;
using HarmonyLib;
using LiteDB;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;




namespace Essentials.NativeModules.Vault.data
{
    internal static class VaultDataManager
    {
        internal static bool Ready { get; set; }
        private static HashSet<PlayerVault> Json_Collection { get; set; } = new();
        private static HashSet<PlayerVault> MigrateCollection { get; set; } = new();

        private const string LiteDB_TableName = "vault";

        private const string Json_FileName = "vault.json";

        private static JsonDataStore<HashSet<PlayerVault>> Json_DataStore { get; set; }

        private static string MySql_TableName => $"{DatabaseManager.MySql_TableName}";

        private const string MySql_CreateTableQuery =
            "`Id` INT NOT NULL AUTO_INCREMENT, " +
            "`SteamId` VARCHAR(32) NOT NULL DEFAULT '0', " +
            "`VaultName` VARCHAR(255) NOT NULL DEFAULT 'N/A', " +
            "`VaultContent` TEXT NOT NULL, " +
            "`LastUpdated` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP," +
            "PRIMARY KEY (Id)";


   


        internal static void Initialize()

        {
            try
            {
                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<HashSet<PlayerVault>>(UEssentials.DataFolder, Json_FileName); ;
                        JSON_Reload();
                        Init();
                        break;
                    case EDatabase.LITEDB:
                        LiteDB_Init();
                        break;
                    case EDatabase.MYSQL:
                        MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                        Init();
                        break;
                }

                Ready = true;
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager Initializing: {e.Message} {e}");

            }
        }
        private static void LiteDB_Init()
        {
            using (var db = new LiteDB.LiteDatabase(DatabaseManager.LiteDB_ConnectionString))
            {
                if (db.UserVersion == 0)
                {
                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                    col.EnsureIndex(x => x.Id);
                    col.EnsureIndex(x => x.SteamId);
                    col.EnsureIndex(x => x.VaultName);
                    col.EnsureIndex(x => x.LastUpdated);
                    db.UserVersion = 1;
                }

                if (db.UserVersion == 1)
                {
                    // migración intermedia
                    db.UserVersion = 2;
                }

                if (db.UserVersion == 2)
                {
                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                    int count = 0;
                    foreach (var playerVault in col.FindAll())
                    {
                        var id = playerVault.Id;
                        playerVault.Id = ++count;
                        col.Insert(playerVault);
                        col.Delete(id);
                    }
                    db.UserVersion = 3;
                }
            }
        }


        private static void Init()
        {
            var vaultVersion = VaultVersionManager.Get();
            if (vaultVersion == null)
            {
                vaultVersion = new VaultVersion { DatabaseVersion = 0 };
            }

            if (vaultVersion.DatabaseVersion == 0)
            {
                vaultVersion.DatabaseVersion = 1;
            }

            if (vaultVersion.DatabaseVersion == 1)
            {
                vaultVersion.DatabaseVersion = 2;
                // switch (EssCore.Instance.Config.Vaultconfig.Database)
                // {
                //     case EDatabase.JSON:
                //         foreach (var playerVault in Json_Collection)
                //         {
                //             playerVault.Id = playerVault.GetHashCode();
                //         }
                //
                //         Json_DataStore.Save(Json_Collection);
                //         break;
                //     case EDatabase.MYSQL:
                //         var task = Task.Run(async () =>
                //         MySQL_LoadAllAsync());
                //         var all = task.GetAwaiter().GetResult();
                //         foreach (var playerVault in all)
                //         {
                //             var id = playerVault.Id;
                //             playerVault.Id = playerVault.GetHashCode();
                //             var task2 = Task.Run(async () => await UpdateAsync(id, playerVault));
                //             task2.GetAwaiter().GetResult();
                //         }
                //
                //         break;
                // }
                // Task.Run(async () => await VaultVersionManager.SetAsync(vaultVersion));
            }

            if (vaultVersion.DatabaseVersion == 2)
            {
                vaultVersion.DatabaseVersion = 3;
                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.JSON:
                        var count = 0;
                        foreach (var playerVault in Json_Collection)
                        {
                            playerVault.Id = ++count;
                        }

                        Json_DataStore.Save(Json_Collection);
                        break;
                    case EDatabase.MYSQL:
                        count = 0;
                        var task = Task.Run(async () => await MySQL_LoadAllAsync());
                        var all = task.GetAwaiter().GetResult();
                        foreach (var playerVault in all)
                        {
                            var id = playerVault.Id;
                            playerVault.Id = ++count;
                            var task2 = Task.Run(async () => await UpdateAsync(id, playerVault));
                            task2.GetAwaiter().GetResult();
                        }

                        break;
                }

                Task.Run(async () => await VaultVersionManager.SetAsync(vaultVersion));
            }
        }

        private static int Json_NewId()
        {
            return (Json_Collection.Max(x => x.Id as int?) ?? 0) + 1;
        }

        private static void JSON_Reload(bool migrate = false)
        {
            try
            {
                if (migrate)
                {
                    MigrateCollection = Json_DataStore.Load() ?? new HashSet<PlayerVault>();
                    return;
                }

                Json_Collection = Json_DataStore.Load() ?? new HashSet<PlayerVault>();
                Json_DataStore.Save(Json_Collection);
            }
            catch (Exception e)
            {
                if (EssCore.Instance.Config.Vaultconfig.DebugMode) EssCore.print($"VaultDataManager JSON_Reload: {e.Message} {e}");

            }
        }

        private static void MySQL_CreateTable(string tableName, string createTableQuery)
        {
            try
            {
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    connection.Execute($"CREATE TABLE IF NOT EXISTS `{tableName}` ({createTableQuery});");
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager MySQL_CreateTable: {e.Message} {e}");

            }
        }

        private static async Task<HashSet<PlayerVault>> LiteDB_LoadAllAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    var result = new HashSet<PlayerVault>();
                    using (var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString))
                    {
                        var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                        foreach (var playerVault in col.FindAll())
                        {
                            result.Add(playerVault);
                        }
                    }

                    return result;
                });
            }
            catch (Exception e)
            {
                EssCore.print($"LiteDB_LoadAllAsync Error: {e.Message}\n{e}");
                return new HashSet<PlayerVault>();
            }
        }


        private static async Task<HashSet<PlayerVault>> MySQL_LoadAllAsync()
        {
            try
            {
                var result = new HashSet<PlayerVault>();
                using (var connection =
                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                {
                    var loadQuery = $"SELECT * FROM `{MySql_TableName}`;";
                    var databases = await connection.QueryAsync(loadQuery);
                    var all = from database in databases.Cast<IDictionary<string, object>>()
                              let byteArray = database["VaultContent"].ToString().ToByteArray()
                              let vaultContent = ItemsWrapper.Deserialize(byteArray)
                              select new PlayerVault
                              {
                                  Id = Convert.ToInt32(database["Id"]),
                                  SteamId = Convert.ToUInt64(database["SteamId"]),
                                  VaultName = database["VaultName"].ToString(),
                                  VaultContent = vaultContent,
                                  LastUpdated = Convert.ToDateTime(database["LastUpdated"]),
                              };
                    foreach (var playerVault in all)
                    {
                        result.Add(playerVault);
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager MySQL_LoadAllAsync: {e.Message} {e}");

                return new HashSet<PlayerVault>();
            }
        }

        internal static async Task AddAsync(PlayerVault playerVault)
        {
            try
            {
                PlayerVault existingVault;

                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.LITEDB:
                        await Task.Run(() =>
                        {
                            using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);

                            existingVault = col
                                .FindOne(x => x.SteamId == playerVault.SteamId && x.VaultName == playerVault.VaultName);

                            if (existingVault != null)
                                return;

                            col.Insert(playerVault);
                        });
                        break;

                    case EDatabase.JSON:
                        var flag = Json_Collection.TryGetValue(playerVault, out _);
                        if (flag)
                            return;

                        playerVault.Id = Json_NewId();
                        Json_Collection.Add(playerVault);
                        await Json_DataStore.SaveAsync(Json_Collection);
                        break;

                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var existing = await connection.ExecuteScalarAsync<bool>(
                                $"SELECT COUNT(DISTINCT 1) FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;",
                                new { playerVault.SteamId, playerVault.VaultName });

                            if (existing)
                                return;

                            var serialized = playerVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();

                            var insertQuery =
                                $"INSERT INTO `{MySql_TableName}` (`SteamId`, `VaultName`, `VaultContent`) " +
                                "VALUES(@SteamId, @VaultName, @VaultContent); SELECT last_insert_id();";

                            var parameter = new DynamicParameters();
                            parameter.Add("@SteamId", playerVault.SteamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);

                            await connection.ExecuteScalarAsync<int>(insertQuery, parameter);
                        }

                        break;
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager AddAsync: {e.Message} {e}");
            }
        }

        internal static async Task<PlayerVault> Get(ulong steamId, string vaultName)
        {
            try
            {
                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.JSON:
                        Json_Collection.TryGetValue(new PlayerVault { SteamId = steamId, VaultName = vaultName },
                            out var value);
                        return value;
                    case EDatabase.LITEDB:
                        return await Task.Run(() =>
                        {
                            using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            return col.Query()
                                      .Where(x => x.SteamId == steamId && x.VaultName == vaultName)
                                      .FirstOrDefault();
                        });

                    case EDatabase.MYSQL:
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var parameter = new DynamicParameters();
                            parameter.Add("@SteamId", steamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", vaultName, DbType.String, ParameterDirection.Input);
                            var query =
                                $"SELECT * FROM `{MySql_TableName}` WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;";
                            var result = connection.Query(query, parameter)
                                .Cast<IDictionary<string, object>>();
                            var first = result.FirstOrDefault();
                            if (first == null)
                                return null;

                            var byteArray = first["VaultContent"].ToString().ToByteArray();
                            var vaultContent = ItemsWrapper.Deserialize(byteArray);
                            return new PlayerVault
                            {
                                Id = Convert.ToInt32(first["Id"]),
                                SteamId = Convert.ToUInt64(first["SteamId"]),
                                VaultName = first["VaultName"]?.ToString(),
                                VaultContent = vaultContent,
                                LastUpdated = Convert.ToDateTime(first["LastUpdated"]),
                            };
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager Get: {e.Message} {e}");

                return null;
            }
        }

        internal static async Task<bool> UpdateAsync(PlayerVault playerVault)
        {
            try
            {
                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.LITEDB:
                        return await Task.Run(() =>
                        {
                            using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            return col.Update(playerVault);
                        });

                    case EDatabase.JSON:
                        return await Json_DataStore.SaveAsync(Json_Collection);
                    case EDatabase.MYSQL:
                        int result;
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var serialized = playerVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();
                            var parameter = new DynamicParameters();
                            // parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                            parameter.Add("@SteamId", playerVault.SteamId, DbType.String, ParameterDirection.Input);
                            parameter.Add("@VaultName", playerVault.VaultName, DbType.String, ParameterDirection.Input);
                            var updateQuery =
                                $"UPDATE {MySql_TableName} SET `VaultContent` = @VaultContent WHERE `SteamId` = @SteamId AND `VaultName` = @VaultName;";
                            result = await connection.ExecuteAsync(updateQuery, parameter);
                        }

                        return result == 1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager UpdateAsync: {e.Message} {e}");

                return false;
            }
        }

        internal static async Task<bool> UpdateAsync(int oldId, PlayerVault playerVault)
        {
            try
            {
                switch (EssCore.Instance.Config.Vaultconfig.Database)
                {
                    case EDatabase.LITEDB:
                        return await Task.Run(() =>
                        {
                            using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                            var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                            col.Delete(oldId);
                            return col.Insert(playerVault);
                        });

                    case EDatabase.JSON:
                        return await Json_DataStore.SaveAsync(Json_Collection);
                    case EDatabase.MYSQL:
                        int result;
                        using (var connection =
                               new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                        {
                            var serialized = playerVault.VaultContent.Serialize();
                            var vaultContent = serialized.ToBase64();
                            var parameter = new DynamicParameters();
                            parameter.Add("@OldId", oldId, DbType.Int32, ParameterDirection.Input);
                            parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                            parameter.Add("@VaultContent", vaultContent, DbType.String, ParameterDirection.Input);
                            var updateQuery =
                                $"UPDATE {MySql_TableName} SET  `Id` = @Id, `VaultContent` = @VaultContent WHERE `Id` = @OldId;";
                            result = await connection.ExecuteAsync(updateQuery, parameter);
                        }

                        return result == 1;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager UpdateAsync: {e.Message} {e}");

                return false;
            }
        }

        internal static async Task MigrateAsync(EDatabase from, EDatabase to)
        {
            try
            {
                switch (from)
                {
                    case EDatabase.LITEDB:
                        MigrateCollection = await LiteDB_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.JSON:
                                Json_DataStore =
                                    new JsonDataStore<HashSet<PlayerVault>>(UEssentials.DataFolder, Json_FileName); ;
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await connection.ExecuteAsync(deleteQuery);

                                    foreach (var playerVault in MigrateCollection)
                                    {
                                        var serialized = playerVault.VaultContent.Serialize();
                                        var vaultContent = serialized.ToBase64();
                                        var parameter = new DynamicParameters();
                                        parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VaultName`, `VaultContent`) " +
                                            "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                        await connection.ExecuteAsync(insertQuery, parameter);
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.JSON:
                        Json_DataStore = new JsonDataStore<HashSet<PlayerVault>>(UEssentials.DataFolder, Json_FileName);
                        JSON_Reload(true);
                        switch (to)
                        {
                            case EDatabase.LITEDB:
                                await Task.Run(() =>
                                {
                                    using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                    col.DeleteAll();
                                    col.InsertBulk(MigrateCollection);
                                });
                                break;

                            case EDatabase.MYSQL:
                                MySQL_CreateTable(MySql_TableName, MySql_CreateTableQuery);
                                using (var connection =
                                       new MySql.Data.MySqlClient.MySqlConnection(DatabaseManager.MySql_ConnectionString))
                                {
                                    var deleteQuery = $"DELETE FROM `{MySql_TableName}`;";
                                    await connection.ExecuteAsync(deleteQuery);

                                    foreach (var playerVault in MigrateCollection)
                                    {
                                        var serialized = playerVault.VaultContent.Serialize();
                                        var vaultContent = serialized.ToBase64();
                                        var parameter = new DynamicParameters();
                                        parameter.Add("@Id", playerVault.Id, DbType.Int32, ParameterDirection.Input);
                                        parameter.Add("@SteamId", playerVault.SteamId, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultName", playerVault.VaultName, DbType.String,
                                            ParameterDirection.Input);
                                        parameter.Add("@VaultContent", vaultContent ?? string.Empty, DbType.String,
                                            ParameterDirection.Input);
                                        var insertQuery =
                                            $"INSERT INTO `{MySql_TableName}` (`Id`, `SteamId`, `VaultName`, `VaultContent`) " +
                                            "VALUES(@Id, @SteamId, @VaultName, @VaultContent);";
                                        await connection.ExecuteAsync(insertQuery, parameter);
                                    }
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    case EDatabase.MYSQL:
                        MigrateCollection = await MySQL_LoadAllAsync();
                        switch (to)
                        {
                            case EDatabase.LITEDB:
                                await Task.Run(() =>
                                {
                                    using var db = new LiteDB.LiteDatabase(DatabaseManager.MySql_ConnectionString);
                                    var col = db.GetCollection<PlayerVault>(LiteDB_TableName);
                                    col.DeleteAll();
                                    col.InsertBulk(MigrateCollection);
                                });
                                break;

                            case EDatabase.JSON:
                                Json_DataStore =
                                new JsonDataStore<HashSet<PlayerVault>>(UEssentials.DataFolder, Json_FileName);
                                await Json_DataStore.SaveAsync(MigrateCollection);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(to), to, null);
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(from), from, null);
                }

                MigrateCollection.Clear();
                MigrateCollection.TrimExcess();
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager MigrateAsync: {e.Message} {e}");

            }

            MigrateCollection.Clear();
            MigrateCollection.TrimExcess();
        }

        internal static async Task AviMigrateAsync(string aviVaultMySQLConnectionString, string aviVaultMySQLTableName)
        {
            try
            {
                var aviVaults = new List<AviVault>();
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(aviVaultMySQLConnectionString))
                {
                    var result = await connection.QueryAsync($"SELECT * FROM {aviVaultMySQLTableName};");
                    var databases = result?.Cast<IDictionary<string, object>>();
                    if (databases == null)
                        return;

                    foreach (var database in databases)
                    {
                        aviVaults.Add(new AviVault
                        {
                            Id = Convert.ToInt32(database["Id"]),
                            OwnerId = database["OwnerId"].ToString(),
                            VaultName = database["VaultName"].ToString(),
                            StorageState = database["StorageState"] as byte[]
                        });
                    }
                }

                var lockerAsset = Assets.find(EAssetType.ITEM, 328) as ItemStorageAsset;
                var lockerAssetCopy = lockerAsset.Copy();
                var traverse = Traverse.Create(lockerAssetCopy);
                var interactableStorage = new InteractableStorage();
                foreach (var aviVault in aviVaults)
                {
                    var vault = Essentials.NativeModules.Vault.Models.Vault.Parse(aviVault.VaultName);
                    traverse.Field("_storage_x").SetValue(vault.Width);
                    traverse.Field("_storage_y").SetValue(vault.Height);
                    interactableStorage.updateState(lockerAssetCopy, aviVault.StorageState);
                    await AddAsync(new PlayerVault
                    {
                        SteamId = ulong.Parse(aviVault.OwnerId),
                        VaultName = vault.Name,
                        VaultContent = ItemsWrapper.Create(interactableStorage.items)
                    });
                }
            }
            catch (Exception e)
            {
                EssCore.print($"VaultDataManager MigrateAsync: {e.Message} {e}");

            }

            MigrateCollection.Clear();
            MigrateCollection.TrimExcess();
        }
    }
}

