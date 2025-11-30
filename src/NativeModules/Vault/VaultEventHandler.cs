using Essentials.Core;
using Essentials.NativeModules.Vault.Utils;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;



// ReSharper disable InconsistentNaming

namespace Essentials.NativeModules.Vault.EventListeners
{
    // Reemplaza todos los mensajes de Logger.LogError y comentarios de Logger.LogWarning por EssCore.print
    // También corrige la línea de error en OnPreItemTook

    public static class PlayerEvent
    {
        public static void OnPreItemTook(Player uplayer, byte x, byte y, uint instanceID, byte to_x, byte to_y,
            byte to_rot, byte to_page, ItemData itemData, ref bool shouldAllow)
        {
            if (!shouldAllow)
                return;
            // EssCore.print("debug", $"[DEBUG] OnPreItemTook {uplayer.channel.owner.playerID.characterName} x:{x} y:{y} instanceID:{instanceID} to_x:{to_x} to_y:{to_y} to_rot:{to_rot} to_page:{to_page} id:{itemData.item.id} allow:{shouldAllow}");
            try
            {
                var player = UnturnedPlayer.FromPlayer(uplayer);
                var cPlayer = player.GetVaultPlayerComponent();
                if (cPlayer.PlayerVault == null)
                    return;

                if (cPlayer.IsBusy)
                {
                    shouldAllow = false;
                    return;
                }

                if (to_page != PlayerInventory.STORAGE)
                    return;

                shouldAllow = !VaultUtil.IsBlacklisted(player, itemData.item.id);
            }
            catch (Exception e)

            {
                if (EssCore.Instance.Config.Vaultconfig.DebugMode) EssCore.print($"PlayerEvent_OnPreItemTook_Error {e.Message}  {e}");
                shouldAllow = true;
            }
        }

        public static void OnPreItemDragged(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte page_1,
            byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            if (!shouldAllow)
                return;
            // EssCore.print("debug", $"[DEBUG] OnPreItemDragged {inventory.channel.owner.playerID.characterName} page_0:{page_0} x_0:{x_0} y_0:{y_0} page_1:{page_1} x_1:{x_1} y_1:{y_1} rot_1:{rot_1} shouldAllow:{shouldAllow}");
            try
            {
                var player = UnturnedPlayer.FromPlayer(inventory.player);
                var cPlayer = player.GetVaultPlayerComponent();
                // Allow if player is not accessing virtual locker
                if (cPlayer.PlayerVault == null)
                    return;

                if (cPlayer.IsBusy)
                {
                    shouldAllow = false;
                    return;
                }

                // Bug fix: Disallow if player storing primary/secondary item to storage or vice versa
                if ((page_0 == 0 || page_0 == 1) && page_1 == 7 || (page_1 == 0 || page_1 == 1) && page_0 == 7)
                {
                    shouldAllow = false;
                    return;
                }

                // Allow if player is not dealing with Storage page
                if (page_1 != PlayerInventory.STORAGE)
                    return;

                var index = inventory.items[page_0].getIndex(x_0, y_0);
                if (index == byte.MaxValue)
                    return;

                var itemJar = inventory.items[page_0].getItem(index);
                if (itemJar == null)
                    return;

                // Disallow if item is in blacklist
                shouldAllow = !VaultUtil.IsBlacklisted(player, itemJar.item.id);
            }
            catch (Exception e)
            {
                if (EssCore.Instance.Config.Vaultconfig.DebugMode) EssCore.print($"PlayerEvent OnPreItemDragged {e.Message}  {e}");

            }
        }

        public static void OnPreItemSwapped(PlayerInventory inventory, byte page_0, byte x_0, byte y_0, byte rot_0,
            byte page_1, byte x_1, byte y_1, byte rot_1, ref bool shouldAllow)
        {
            if (!shouldAllow)
                return;
            // EssCore.print("debug", $"[DEBUG] OnPreItemSwapped {inventory.channel.owner.playerID.characterName} page_0:{page_0} x_0:{x_0} y_0:{y_0} rot_0:{rot_0} page_1:{page_1} x_1:{x_1} y_1:{y_1} rot_1:{rot_1} shouldAllow:{shouldAllow}");
            try
            {
                var player = UnturnedPlayer.FromPlayer(inventory.player);
                var cPlayer = player.GetVaultPlayerComponent();

                // Allow if player is not accessing virtual locker
                if (cPlayer.PlayerVault == null)
                    return;

                if (cPlayer.IsBusy)
                {
                    shouldAllow = false;
                    return;
                }

                // Bug fix: Disallow if player storing primary/secondary item to storage or vice versa
                if ((page_0 == 0 || page_0 == 1) && page_1 == 7 || (page_1 == 0 || page_1 == 1) && page_0 == 7)
                {
                    shouldAllow = false;
                    return;
                }

                // Run if player is not dealing with Storage page
                if (page_1 != PlayerInventory.STORAGE)
                    return;

                var index = inventory.items[page_0].getIndex(x_0, y_0);
                if (index == byte.MaxValue)
                    return;

                var itemJar = inventory.items[page_0].getItem(index);
                if (itemJar == null)
                    return;

                // Disallow if item is in blacklist
                shouldAllow = !VaultUtil.IsBlacklisted(player, itemJar.item.id);
            }
            catch (Exception e)
            {
                if (EssCore.Instance.Config.Vaultconfig.DebugMode) EssCore.print($"PlayerEvent OnPreItemSwapped {e.Message}  {e}");
            }
        }
    }
}