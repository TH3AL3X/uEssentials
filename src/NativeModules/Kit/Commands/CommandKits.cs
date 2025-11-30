using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Essentials.I18n;
using SDG.Unturned;
using Steamworks;
using Essentials.Api.Unturned;

namespace Essentials.NativeModules.Kit.Commands
{
    [CommandInfo(
        Name = "kits",
        Description = "View available kits"
    )]
    public class CommandKits : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource source, ICommandArgs parameters)
        {
            var kitConfig = EssCore.Instance.Config.Kit;
            var hasEconomyProvider = UEssentials.EconomyProvider.IsPresent;

            var kits = KItModule.Instance.KitManager.Kits
                .Where(k => k.CanUse(source))
                .Select(k =>
                {
                    string coldown = string.Empty;
                    string cooldownColor = string.Empty;

                    // Obtener datos del jugador sólo si es necesario (cuando no es consola/admin)
                    ulong? playerId = null;
                    int playerRepu = 0;
                    if (!source.IsAdmin && !source.IsConsole)
                    {
                        playerId = source.ToPlayer().CSteamId.m_SteamID;
                        playerRepu = UPlayer.From(playerId.Value).RocketPlayer.Reputation;
                    }

                    // Comprobación de reputación (independiente del cooldown)
                    if (k.Repuacces != 0 && playerId.HasValue)
                    {
                        bool meets;
                        if (k.Repuacces > 0)
                            meets = playerRepu >= k.Repuacces; // requiere reputación mínima
                        else
                            meets = playerRepu <= k.Repuacces; // requiere reputación máxima/menor o igual (valor negativo)

                        if (!meets)
                        {
                            // Mostrar kit como no disponible por reputación (no mutar k.Messageaddon)
                            return $"{k.Messageaddon}<color=#b19500>{k.Name}</color>";
                        }
                    }

                    // Manejo de cooldown (si corresponde y si hay un jugador real)
                    if (!source.IsAdmin && !source.IsConsole && k.Cooldown > 0 && playerId.HasValue)
                    {
                        Commands.CommandKit.Cooldowns.TryGetValue(playerId.Value, out var playerCooldowns);
                        playerCooldowns ??= new Dictionary<string, DateTime>();
                        Commands.CommandKit.Cooldowns[playerId.Value] = playerCooldowns;

                        if (playerCooldowns.TryGetValue(k.Name, out var lastUsed))
                        {
                            double remaining = k.Cooldown - (DateTime.Now - lastUsed).TotalSeconds;

                            if (remaining > 0)
                            {
                                cooldownColor = $"<color={kitConfig.coldownkitcolor}>"; // Asignar color configurado
                                if (kitConfig.showcoldowntime)
                                    coldown = $"<color=#ffffff> [<color={kitConfig.coldownkitcolor}>{FormatShortTime((uint)Math.Ceiling(remaining))}</color><color=#ffffff>]</color>";
                                else
                                    coldown = $"<color=#ffffff> [<color={kitConfig.coldownkitcolor}>en cooldown</color><color=#ffffff>]</color>";
                            }
                        }
                    }

                    // Manejo de costo y formato final
                    var display = k.Messageaddon + cooldownColor + k.Name + coldown;

                    if (!hasEconomyProvider || !kitConfig.ShowCost || (k.Cost <= 0 && !kitConfig.ShowCostIfZero))
                        return display;

                    return string.Format(kitConfig.CostFormat, display, k.Cost, UEssentials.EconomyProvider.Value.CurrencySymbol);
                })
                .ToList();

            // Si no hay kits disponibles
            if (kits.Count == 0)
            {
                if (KItModule.Instance.KitManager.Count == 0)
                    return CommandResult.LangError("icon_error_general", "KIT_NONE_DEFINED");

                EssLang.SendNoBuffer(source, "KIT_NONE");
                return CommandResult.Success();
            }

            // Enviar lista de kits
            EssLang.Send("icon_KIT_LIST", source, "KIT_LIST", string.Join(", ", kits));
            return CommandResult.Success();
        }

        // Método auxiliar para formatear tiempo
        private static string FormatShortTime(uint seconds)
        {
            uint hours = seconds / 3600;
            uint minutes = (seconds % 3600) / 60;
            uint secs = seconds % 60;

            string result = "";

            if (hours > 0)
                result += $"{hours}h ";
            if (minutes > 0)
                result += $"{minutes}m ";
            if (secs > 0 || result == "")
                result += $"{secs}s";

            return result.Trim();
        }
    }
}
