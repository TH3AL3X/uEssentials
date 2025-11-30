using Essentials.NativeModules.Vault.playercomponents;
using Rocket.Unturned.Player;
using System;

namespace Essentials.NativeModules.Vault.Utils
{
    public static class Extensions
    {
        public static VaultPlayerComponent GetVaultPlayerComponent(this UnturnedPlayer player) =>
            player.GetComponent<VaultPlayerComponent>();

        public static string ToBase64(this byte[] byteArray)
        {
            return Convert.ToBase64String(byteArray);
        }

        public static byte[] ToByteArray(this string base64)
        {
            return Convert.FromBase64String(base64);
        }

    }
}