using Cysharp.Threading.Tasks;
using Essentials.src.NativeModules.Vault.Data;
using Newtonsoft.Json;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essentials.NativeModules.Vault.Models
{
    [Serializable]
    public class ItemsWrapper
    {
        public byte Page { get; set; }
        public byte Height { get; set; }
        public byte Width { get; set; }
        public List<ItemJarWrapper2> Items { get; set; } = new List<ItemJarWrapper2>();

        public static ItemsWrapper Deserialize(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                var json = System.Text.Encoding.UTF8.GetString(data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ItemsWrapper>(json);
            }
            catch
            {
                return null;
            }
        }

        public ItemsWrapper(byte page, byte height, byte width, List<ItemJarWrapper2> items)
        {
            Page = page;
            Height = height;
            Width = width;
            Items = items;
        }

        public static ItemsWrapper Create(Items items)
        {
            return new ItemsWrapper(items.page, items.height, items.width,
                items.items.Select(ItemJarWrapper2.Create).ToList());
        }

        public Items ToItems()
        {
            var items = new Items(Page);
            items.resize(Width, Height);
            foreach (var itemJarWrapper in Items)
                items.addItem(itemJarWrapper.X, itemJarWrapper.Y, itemJarWrapper.Rotation,
                    itemJarWrapper.Item.ToItem());

            return items;
        }

        public void LoadVault(UnturnedPlayer player, Essentials.NativeModules.Vault.Models.Vault vault)
        {
            var vaultItems = new Items(Page);
            vaultItems.resize(vault.Width, vault.Height);
            vaultItems.onStateUpdated += () =>
            {
                Items = new List<ItemJarWrapper2>();
                foreach (var itemJar in vaultItems.items)
                {
                    Items.Add(ItemJarWrapper2.Create(itemJar));
                }
                UniTask.Run(async () =>
                {
                    try
                    {
                        await JsonDataStore.UpdateAsync(player.CSteamID.m_SteamID, vault);
                    }
                    catch (Exception exception)
                    {
                        Logger.LogError("[Essentials.NativeModules.Vault.SQDB] [ERROR] VaultManager UpdateAsync: " + exception);
                    }
                }).Forget();
            };
            player.Player.inventory.updateItems(7, vaultItems);
            player.Player.inventory.sendStorage();
        }

        public byte[] Serialize()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}

