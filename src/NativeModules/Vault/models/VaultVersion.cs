using System;

namespace Essentials.NativeModules.Vault.Models
{
    [Serializable]
    public class VaultVersion
    {
        public uint DatabaseVersion { get; set; }
        public VaultVersion()
        {

        }
    }
}