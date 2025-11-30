using Essentials.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Essentials.src.NativeModules.Vault.Data
{
    public static class JsonDataStore
    {
        private static readonly string DataPath = Path.Combine(EssCore.Instance.Directory, "data");

        static JsonDataStore()
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);
        }

        public static async Task UpdateAsync(ulong steamId, Essentials.NativeModules.Vault.Models.Vault vault)
        {
            try
            {
                var filePath = Path.Combine(DataPath, $"{steamId}_{vault.Name}.json");
                var json = JsonConvert.SerializeObject(vault, Formatting.Indented);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating vault data for SteamID {steamId}", ex);
            }
        }

        public static async Task<Essentials.NativeModules.Vault.Models.Vault> GetAsync(ulong steamId, string vaultName)
        {
            try
            {
                var filePath = Path.Combine(DataPath, $"{steamId}_{vaultName}.json");
                if (!File.Exists(filePath))
                    return null;

                var json = await File.ReadAllTextAsync(filePath);
                return JsonConvert.DeserializeObject<Essentials.NativeModules.Vault.Models.Vault>(json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading vault data for SteamID {steamId}", ex);
            }
        }
    }
}