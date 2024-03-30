using Essentials.Components.Player;
using HarmonyLib;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Text;

namespace Essentials.src.Patches
{
    internal class ZombieIgnorePatch
    {
        [HarmonyPatch(typeof(Zombie))]
        [HarmonyPatch("alert")]
        [HarmonyPatch(new Type[]
               {
            typeof(Player)
               })]
        internal static class AlertPatch
        {
            private static bool Prefix(Player newPlayer)
            {
                return !newPlayer.GetComponent<ZombieIgnore>().ignore_zombies;
            }
        }
    }
}
