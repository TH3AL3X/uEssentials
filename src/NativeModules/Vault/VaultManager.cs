using Essentials.Api;
using Essentials.Core;
using Essentials.NativeModules.Vault.data;
using Essentials.NativeModules.Vault.Models;
using Essentials.NativeModules.Vault.playercomponents;
using Rocket.API.Extensions;
using SDG.Unturned;
using System;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;


namespace Essentials.NativeModules.Vault.Vaultmanager
{

    public sealed class VaultManager
    {
        private const int Major = 1;
        private const int Minor = 2;
        private const int Patch = 4;

        public static VaultManager Inst;

        public void Load()
        {
            var config = EssCore.Instance.Config.Vaultconfig;
            try
            {
                Inst = this;

                DatabaseManager.Initialize();
                VaultVersionManager.Initialize();
                VaultDataManager.Initialize();

                if (Level.isLoaded)
                {
                    foreach (var steamPlayer in Provider.clients)
                        steamPlayer.player.gameObject.TryAddComponent<VaultPlayerComponent>();
                }
                UEssentials.Logger.LogInfo($"[{nameof(VaultManager)}] using {config.Database}");
            }
            catch (Exception ex)
            {
                UEssentials.Logger.LogError("An error ocurred while loading Vaults...");
                UEssentials.Logger.LogException(ex);
            }
        }

        public void safesave()
        {
            try
            {
               

            }
            catch (Exception ex)
            {
                UEssentials.Logger.LogError("An error ocurred while saving Vaults...");
                UEssentials.Logger.LogException(ex);
            }
        }
        public void Unload()
        {
            safesave();
            var config = EssCore.Instance.Config.Vaultconfig;
                foreach (var steamPlayer in Provider.clients)
                    steamPlayer.player.gameObject.TryRemoveComponent<VaultPlayerComponent>();
            
            Inst = null;
            UEssentials.Logger.LogWarning($"[{nameof(VaultManager)}] Plugin unloaded successfully!");
        }
    }
}
public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance;

    void Awake()
    {
        Instance = this;
    }
}
