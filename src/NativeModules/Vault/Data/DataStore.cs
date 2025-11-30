using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Rocket.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Essentials.NativeModules.Vault.data
{
    public class JsonDataStore<T> where T : class
    {
        private string DataPath { get; set; }

        public JsonDataStore(string dir, string fileName)
        {
            DataPath = Path.Combine(dir, fileName);
        }

        public bool Save(T obj)
        {
            try
            {
                var objData = JsonConvert.SerializeObject(obj, Formatting.Indented);

                using (var stream = new StreamWriter(DataPath, false))
                {
                    stream.Write(objData);
                }

                return true;
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ERROR] JSON Save: {exception}");
                return false;
            }
        }

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks = new();

        public async UniTask<bool> SaveAsync(T obj)
        {
            var fileLock = FileLocks.GetOrAdd(DataPath, _ => new SemaphoreSlim(1, 1));
            await fileLock.WaitAsync();
            try
            {
                var objData = JsonConvert.SerializeObject(obj, Formatting.Indented);
                using (var stream = new StreamWriter(DataPath, false))
                {
                    await stream.WriteAsync(objData);
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"[ERROR] JSON SaveAsync: {ex}");
                return false;
            }
            finally
            {
                fileLock.Release();
            }
        }

        public T Load()
        {
            if (!File.Exists(DataPath))
                return null;
            string dataText;
            using (var stream = File.OpenText(DataPath))
            {
                dataText = stream.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<T>(dataText);
        }

        public async UniTask<T> LoadAsync()
        {
            if (!File.Exists(DataPath))
                return null;
            string dataText;
            using (var stream = File.OpenText(DataPath))
            {
                dataText = await stream.ReadToEndAsync();
            }

            return JsonConvert.DeserializeObject<T>(dataText);
        }

        internal async Task UpdateAsync(ulong m_SteamID, Models.Vault vault)
        {
           throw new NotImplementedException();
        }
    }
}