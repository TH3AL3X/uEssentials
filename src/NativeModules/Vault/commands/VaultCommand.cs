using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using Essentials.Logging;
using Essentials.NativeModules.Vault.Models;
using Essentials.NativeModules.Vault.Utils;
using Rocket.Unturned.Player;
using System.Linq;

namespace Essentials.NativeModules.Vault.Commands
{
    [CommandInfo(
        Name = "vault",
        Description = "Open a virtual vault storage",
        Usage = "[vaultName]",
        Aliases = new[] { "locker" },
        Permission = "vault",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandVault : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var Uplayer = UnturnedPlayer.FromCSteamID(player.CSteamId);
            var cPlayer = Vault.Utils.Extensions.GetVaultPlayerComponent(Uplayer);

            if (args.Length > 1)
                return CommandResult.LangError("icon_error_general", "invalid_parameter", Usage);

            if (cPlayer.IsBusy)
                return CommandResult.LangError("icon_error_general", "vault_system_busy");

            if (player.IsInVehicle)
                return CommandResult.LangError("icon_error_general", "in_vehicle");


            Models.Vault vault = null;

            if (args.Length == 0)
            {
                vault = cPlayer.SelectedVault ?? VaultUtil.GetVaults(Uplayer).OrderByDescending(v => v.Width * v.Height).FirstOrDefault();
                if (vault == null)
                    return CommandResult.LangError("icon_error_general", "no_permission_all");
                cPlayer.SelectedVault = vault;
            }
            else
            {
                vault = Models.Vault.Parse(args.RawArguments[0]);
                if (vault == null)
                    return CommandResult.LangError("icon_error_general", "vault_not_found");

                if (!player.HasPermission(vault.Permission))
                    return CommandResult.LangError("icon_error_general", "no_permission", vault.Permission);

                cPlayer.SelectedVault = vault;
            }

            if (cPlayer.PlayerVaultItems != null)
                return CommandResult.LangError("icon_error_general", "vault_processing");

            if (VaultUtil.IsVaultBusy(Uplayer.CSteamID.m_SteamID, vault))
                return CommandResult.LangError("icon_error_general", "vault_busy");

            // Aquí está la magia: la llamada async interna
            try
            {
                VaultUtil.OpenVaultAsync(Uplayer, vault).GetAwaiter().GetResult();
            }
            catch (System.Exception ex)
            {
                UEssentials.Logger.LogError($"[Vault] Error al abrir la bóveda para {player.DisplayName} ({Uplayer.CSteamID.m_SteamID})");
                UEssentials.Logger.LogException(ex);
                return CommandResult.LangError("icon_error_general", "vault_open_error");
            }

            return CommandResult.Success();
        }
    }
}
