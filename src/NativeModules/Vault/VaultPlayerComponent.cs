using Essentials.Core;
using Essentials.NativeModules.Vault.data;
using Essentials.NativeModules.Vault.Models;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Essentials.NativeModules.Vault.playercomponents
{
    public class VaultPlayerComponent : UnturnedPlayerComponent
    {
        internal bool IsBusy { get; set; }
        internal Models.Vault SelectedVault { get; set; }
        internal PlayerVault PlayerVault { get; set; }
        internal Items PlayerVaultItems { get; set; }

        protected override void Load()
        {
            Player.Player.inventory.onInventoryResized += OnInventoryResized;

        }

        protected override void Unload()
        {
            Player.Player.inventory.onInventoryResized -= OnInventoryResized;

            if (PlayerVault != null && PlayerVaultItems != null)
            {
                EssCore.print($"[Vault] Forzando guardado de vault al salir {Player.CharacterName}");

                var itemsWrapper = ItemsWrapper.Create(PlayerVaultItems);
                PlayerVault.VaultContent = itemsWrapper;

                // Guardar de forma sincrónica antes de liberar
                VaultDataManager.UpdateAsync(PlayerVault).Wait();
            }

            PlayerVaultItems?.clear();
            PlayerVaultItems?.items.TrimExcess();
            PlayerVault = null;
            PlayerVaultItems = null;
            IsBusy = false;
        }

        private void OnInventoryResized(byte page, byte newwidth, byte newheight)
        {
            if (EssCore.Instance.Config.Vaultconfig.DebugMode)
                EssCore.print($"PlayerEvent OnPreItemSwapped {page} {newwidth} {newheight} {Player.CharacterName}");

            if (page == PlayerInventory.STORAGE && newwidth == 0 && newheight == 0 && PlayerVault != null && PlayerVaultItems != null)
            {
                IsBusy = true;

                var itemsWrapper = ItemsWrapper.Create(PlayerVaultItems);
                PlayerVault.VaultContent = itemsWrapper;

                DatabaseManager.Queue.Enqueue(async () =>
                {
                    await VaultDataManager.UpdateAsync(PlayerVault);
                    PlayerVaultItems.clear();
                    PlayerVaultItems.items.TrimExcess();
                    PlayerVault = null;
                    PlayerVaultItems = null;
                    IsBusy = false;
                });
            }
        }
    }
}
