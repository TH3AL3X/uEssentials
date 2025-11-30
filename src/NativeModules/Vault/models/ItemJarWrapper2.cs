using SDG.Unturned;
using System;

namespace Essentials.NativeModules.Vault.Models
{
    [Serializable]
    public class ItemJarWrapper2
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Rotation { get; set; }
        public ItemWrapper Item { get; set; } = new ItemWrapper();

        public ItemJarWrapper2()
        {

        }
        public ItemJarWrapper2(byte x, byte y, byte rotation, ItemWrapper item)
        {
            X = x;
            Y = y;
            Rotation = rotation;
            Item = item;
        }

        public static ItemJarWrapper2 Create(ItemJar itemJar)
        {
            return new ItemJarWrapper2(itemJar.x, itemJar.y, itemJar.rot, ItemWrapper.Create(itemJar.item));
        }
    }
}