using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;
using Essentials.NativeModules.Vault.Utils;
using Rocket.Unturned.Player;
using System.Linq;

namespace Essentials.NativeModules.Vault.Commands
{
    [CommandInfo(
        Name = "vaults",
        Description = "List all available vaults",
        Usage = "",
        Aliases = new[] { "lockers" },
        Permission = "vault",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandVaults : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var Uplayer = UnturnedPlayer.FromCSteamID(player.CSteamId);

            var vaults = VaultUtil.GetVaults(Uplayer);
            if (vaults.Count == 0)
            {
                // Si no tiene bóvedas disponibles, enviar mensaje claro
                return CommandResult.LangError("icon_error_general", "vault_not_found");
            }

            var list = string.Join(", ", vaults.Select(v => $"<color={v.Color}>{v.Name ?? "???"}</color>"));

            EssLang.Send("icon_Vault_LIST", src, "Vault_LIST", list);
            return CommandResult.Success();
        }
    }
}
