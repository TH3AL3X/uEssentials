using Essentials.Core;
using Essentials.NativeModules.Vault.data;
using Essentials.NativeModules.Vault.Models;
using Essentials.NativeModules.Vault.playercomponents;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;



namespace Essentials.NativeModules.Vault.Utils
{
    internal static class VaultUtil
    {
        internal static List<Models.Vault> GetVaults(UnturnedPlayer player)
        {
            try
            {
                return EssCore.Instance.Config.Vaultconfig.Vault.Where(vault => player.HasPermission(vault.Permission ?? string.Empty))
                    .ToList();
            }
            catch (Exception e)
            {
                EssCore.print($"VaultUtil GetVaults: {e.Message} {e}");
                return new List<Models.Vault>();
            }
        }

        internal static bool IsBlacklisted(UnturnedPlayer player, ushort id)
        {
            try
            {
                var blacklist = EssCore.Instance.Config.Vaultconfig.BlacklistedItems.Any(blacklistedItem => blacklistedItem.Items.Any(
                    blacklistItemId =>
                        blacklistItemId == id && !player.HasPermission(blacklistedItem.BypassPermission)));
                if (!blacklist)
                    return false;
                var itemAsset = (ItemAsset)Assets.find(EAssetType.ITEM, id);
                EssCore.print(("BLACKLIST".ToString(), itemAsset.itemName, itemAsset.id));
                return true;
            }
            catch (Exception e)
            {
                EssCore.print($"VaultUtil IsBlacklisted: {e.Message} {e}");
                return false;
            }
        }

        internal static async Task OpenVaultAsync(UnturnedPlayer player, Models.Vault vault)
        {
            try
            {
                DatabaseManager.Queue.Enqueue(async () => await VaultDataManager.AddAsync(new PlayerVault
                {
                    SteamId = player.CSteamID.m_SteamID,
                    VaultName = vault.Name
                }));

                await LoadVaultAsync(player, vault);

                if (EssCore.Instance.Config.Vaultconfig.DebugMode)
                {
                    var mensaje = $"{player.CharacterName} is accessing {vault.Name} Vault";
                    EssCore.print(mensaje);
                }
            }
            catch (Exception e)
            {
                EssCore.print($"OpenVaultAsync: {e.Message} {e}");
            }
        }

        internal static void AdminOpenVault(UnturnedPlayer player, PlayerVault playerVault)
        {
            try
            {
                AdminLoadVault(player, playerVault);
                if (EssCore.Instance.Config.Vaultconfig.DebugMode)
                {
                    var mensaje = $"{player.CharacterName} is accessing {playerVault.SteamId}'s {playerVault.VaultName} Vault";
                    Logger.LogWarning(mensaje);
                }
            }
            catch (Exception e)
            {
                EssCore.print($"AdminOpenVault: {e.Message} {e}");
            }
        }

        internal static void OpenVirtualTrash(UnturnedPlayer player)
        {
            try
            {
                var trashItems = new Items(7);
                trashItems.resize(EssCore.Instance.Config.Vaultconfig.Trash.Width, EssCore.Instance.Config.Vaultconfig.Trash.Height);
                player.Player.inventory.updateItems(7, trashItems);
                player.Player.inventory.sendStorage();
            }
            catch (Exception e)
            {
                EssCore.print($"VaultUtil OpenVirtualTrashAsync: {e.Message} {e}");
            }
        }

        private static async Task LoadVaultAsync(UnturnedPlayer player, Models.Vault vault)
        {
            IEnumerator SendItemsOverTime(VaultPlayerComponent component, Items items)
            {
                var toRemove = new List<ItemJarWrapper2>();

                if (component == null || component.PlayerVault == null ||
                    component.PlayerVault.VaultContent == null ||
                    component.PlayerVault.VaultContent.Items == null)
                {
                    EssCore.print("[VaultUtil] component o sus datos son null en SendItemsOverTime");
                    yield break;
                }

                foreach (var itemJarWrapper in component.PlayerVault.VaultContent.Items)
                {
                    if (items.width == 0 || items.height == 0)
                        break;

                    if (EssCore.Instance.Config.Vaultconfig.AutoSortVault)
                    {
                        if (!items.tryAddItem(itemJarWrapper.Item.ToItem()))
                        {
                            ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                            toRemove.Add(itemJarWrapper);
                        }
                    }
                    else
                    {
                        if (itemJarWrapper.X > vault.Width || itemJarWrapper.Y > vault.Height)
                        {
                            ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                            toRemove.Add(itemJarWrapper);
                        }
                        else
                        {
                            items.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                                itemJarWrapper.Item.ToItem());
                        }
                    }

                    yield return null;
                }

                component.IsBusy = false;

                foreach (var itemJarWrapper in toRemove)
                    component.PlayerVault.VaultContent.Items.Remove(itemJarWrapper);
            }

            try
            {
                var cPlayer = player.GetVaultPlayerComponent();
                cPlayer.IsBusy = true;

                var loadedVault = await VaultDataManager.Get(player.CSteamID.m_SteamID, vault.Name);
                cPlayer.PlayerVault = loadedVault;

                var vaultItems = new Items(7);
                vaultItems.resize(vault.Width, vault.Height);
                cPlayer.PlayerVaultItems = vaultItems;

                player.Player.inventory.isStoring = true;
                player.Player.inventory.storage = null;
                player.Player.inventory.updateItems(7, vaultItems);
                player.Player.inventory.sendStorage();

                EssCore.Instance.StartCoroutine(SendItemsOverTime(cPlayer, vaultItems));
            }
            catch (Exception e)
            {
                EssCore.print($"VaultUtil LoadVault: {e.Message} {e}");
            }
        }

        private static void AdminLoadVault(UnturnedPlayer player, PlayerVault playerVault)
        {
            try
            {
                var cPlayer = player.GetVaultPlayerComponent();

                var vaultItems = new Items(7);
                vaultItems.resize(playerVault.VaultContent.Width, playerVault.VaultContent.Height);

                foreach (var itemJarWrapper in playerVault.VaultContent.Items)
                {
                    if (itemJarWrapper.X > playerVault.VaultContent.Width ||
                        itemJarWrapper.Y > playerVault.VaultContent.Height)
                        ItemManager.dropItem(itemJarWrapper.Item.ToItem(), player.Position, true, true, true);
                    else
                        vaultItems.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                            itemJarWrapper.Item.ToItem());
                }

                player.Player.inventory.isStoring = true;
                player.Player.inventory.storage = null;
                player.Player.inventory.updateItems(7, vaultItems);
                player.Player.inventory.sendStorage();
                cPlayer.PlayerVault = playerVault;
                cPlayer.PlayerVaultItems = vaultItems;
            }
            catch (Exception e)
            {
                EssCore.print($"VaultUtil AdminLoadVault: {e.Message} {e}");
            }
        }

        internal static bool IsVaultBusy(ulong owner, Models.Vault vault)
        {
            foreach (var steamPlayer in Provider.clients)
            {
                var cPlayer = steamPlayer.player.GetComponent<Essentials.NativeModules.Vault.playercomponents.VaultPlayerComponent>();
                if (cPlayer.PlayerVault != null && cPlayer.PlayerVaultItems != null &&
                    cPlayer.PlayerVault.SteamId == owner &&
                    cPlayer.PlayerVault.VaultName == vault.Name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
