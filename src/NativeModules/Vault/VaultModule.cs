using Essentials.Api.Module;
using Essentials.NativeModules.Vault.Vaultmanager;
using static Essentials.Api.UEssentials;

namespace Essentials.NativeModules.Vault
{

    [ModuleInfo(Name = "Vault")]
    public class VaultModule : NativeModule
    {

        private const string CommandsNamespace = "Essentials.NativeModules.Vault.Commands";

        public VaultManager VaultManager { get; private set; }
        public static VaultModule Instance { get; private set; }
        public override void OnLoad()
        {
            Instance = this;
            VaultManager = new VaultManager();
            VaultManager.Load();


            CommandManager.RegisterAll(CommandsNamespace);
            EventManager.RegisterAll<VaultManager>();
        }

        public override void OnUnload()
        {
            EventManager.UnregisterAll<VaultManager>();
            CommandManager.UnregisterAll(CommandsNamespace);
        }

    }

}
