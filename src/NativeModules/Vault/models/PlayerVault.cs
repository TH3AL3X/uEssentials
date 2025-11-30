using System;



namespace Essentials.NativeModules.Vault.Models
{
    [Serializable]
    public class PlayerVault
    {
        public int Id { get; set; }
        public ulong SteamId { get; set; }
        public string VaultName { get; set; } = string.Empty;
        public ItemsWrapper VaultContent { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;


        public PlayerVault()
        {

        }

        public Vault GetVault()
        {
            return Vault.Parse(VaultName);
        }

        public override bool Equals(object obj)
        {
            if (obj is not PlayerVault playerVault)
                return false;

            return playerVault.Id == Id || playerVault.SteamId == SteamId && playerVault.VaultName == VaultName;
        }

        protected bool Equals(PlayerVault other)
        {
            return SteamId == other.SteamId && VaultName == other.VaultName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (SteamId.GetHashCode() * 397) ^ (VaultName != null ? VaultName.GetHashCode() : 0);
            }
        }

        public static int HashCode(ulong steamId, string vaultName)
        {
            unchecked
            {
                return (steamId.GetHashCode() * 397) ^ (vaultName?.GetHashCode() ?? 0);
            }
        }
    }

}
