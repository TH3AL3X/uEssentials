namespace Essentials.NativeModules.Vault.Models
{
    public class AviVault
    {
        public virtual int Id { get; set; }
        public virtual string OwnerId { get; set; }
        public virtual string VaultName { get; set; }
        public virtual byte[] StorageState { get; set; }
    }
}