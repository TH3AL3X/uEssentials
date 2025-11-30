using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core;
using Essentials.I18n;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Data;

namespace Essentials.NativeModules.Kit.Commands
{

    [CommandInfo(
        Name = "kit",
        Description = "Get a kit",
        Usage = "[kit_name] <player | *>"
    )]
    public class CommandKit : EssCommand
    {

        /*
            player_id -> [kit_name, last_use]
        */
        internal static Dictionary<ulong, Dictionary<string, DateTime>> Cooldowns =
            new Dictionary<ulong, Dictionary<string, DateTime>>();

        internal static Dictionary<ulong, DateTime> GlobalCooldown =
            new Dictionary<ulong, DateTime>();

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length == 0 || (args.Length == 1 && src.IsConsole))
            {
                return CommandResult.ShowUsage();
            }

            var player = src.ToPlayer();
            var kitName = args[0].ToLowerString;

            if (!KItModule.Instance.KitManager.Contains(kitName))
            {
                return CommandResult.LangError("icon_error_general", "KIT_NOT_EXIST", kitName);
            }

            var requestedKit = KItModule.Instance.KitManager.GetByName(kitName);

            if (!requestedKit.CanUse(player))
            {
                return CommandResult.LangError("icon_error_general", "KIT_NO_PERMISSION");
            }

            var steamPlayerId = player.CSteamId.m_SteamID;
            var kitCost = requestedKit.Cost;

            if (
                kitCost > 0 &&
                UEssentials.EconomyProvider.IsPresent &&
                !src.HasPermission("essentials.bypass.kitcost")
            )
            {
                var ecoProvider = UEssentials.EconomyProvider.Value;

                if (!ecoProvider.Has(player, kitCost))
                {
                    return CommandResult.LangError("icon_error_general", "KIT_NO_MONEY", kitCost, ecoProvider.CurrencySymbol);
                }
            }

            var globalCooldown = EssCore.Instance.Config.Kit.GlobalCooldown;
            var kitCooldown = requestedKit.Cooldown;
            // revisar futuros errores discord ellocoed
            var messageaddon = requestedKit.Messageaddon;

            if (!src.HasPermission("essentials.bypass.kitcooldown"))
            {
                // Check if is on global cooldown
                if (globalCooldown > 0 && GlobalCooldown.ContainsKey(steamPlayerId))
                {
                    var remainingTime = DateTime.Now - GlobalCooldown[steamPlayerId];

                    if ((remainingTime.TotalSeconds + 1) < globalCooldown)
                    {
                        return CommandResult.LangError("icon_error_general", "KIT_GLOBAL_COOLDOWN",
                            TimeUtil.FormatSeconds((uint)(globalCooldown - remainingTime.TotalSeconds)));
                    }
                }

                // Check if is on cooldown for this specific kit
                if (kitCooldown > 0)
                {
                    if (!Cooldowns.TryGetValue(steamPlayerId, out var playerCooldowns) || playerCooldowns == null)
                    {
                        Cooldowns[steamPlayerId] = playerCooldowns = new Dictionary<string, DateTime>();
                    }

                    if (playerCooldowns.TryGetValue(kitName, out var lastTimeUsedThisKit))
                    {
                        var remainingTime = DateTime.Now - lastTimeUsedThisKit;

                        if ((remainingTime.TotalSeconds + 1) < kitCooldown)
                        {
                            return CommandResult.LangError("icon_error_general", "KIT_COOLDOWN", TimeUtil.FormatSeconds(
                                (uint)(kitCooldown - remainingTime.TotalSeconds)));
                        }
                    }
                }
            }

            if (kitCost > 0 && !src.HasPermission("essentials.bypass.kitcost"))
            {
                UEssentials.EconomyProvider.IfPresent(ec =>
                {
                    ec.Withdraw(player, kitCost);
                    EssLang.Send("iconkit-paid", player, "KIT_PAID", kitCost, ec.CurrencySymbol);

                });
            }

            // Si sólo hay 1 argumento -> dar el kit al que lo ejecuta
            if (args.Length == 1)
            {
                var k = requestedKit;
                var playerId = player.CSteamId.m_SteamID;
                var prepu = UPlayer.From(playerId).RocketPlayer.Reputation;

                // Comprobación de reputación requerida (si es distinto de 0)
                if (k.Repuacces != 0)
                {
                    if (k.Repuacces > 0)
                    {
                        // Requiere reputación mínima positiva
                        if (prepu < k.Repuacces)
                        {
                            EssLang.Send("icon_insufficient_positive_reputation", src, "insufficient_positive_reputation", kitName, k.Repuacces);
                            return CommandResult.Empty();
                        }
                    }
                    else // k.Repuacces < 0
                    {
                        // Requiere reputación máxima (valor negativo), bloqueo si el jugador tiene mayor reputación (menos negativo)
                        if (prepu > k.Repuacces)
                        {
                            EssLang.Send("icon_insufficient_negative_reputation",src, "insufficient_negative_reputation", kitName, k.Repuacces);
                            return CommandResult.Empty();
                        }
                    }
                }

                requestedKit.GiveTo(player);

                // Only apply the cooldowns if the player received the kit
                // and does not have the bypass permission.
                if (!src.HasPermission("essentials.bypass.kitcooldown"))
                {
                    if (globalCooldown > 0) GlobalCooldown[steamPlayerId] = DateTime.Now;
                    if (kitCooldown > 0)
                    {
                        if (!Cooldowns.TryGetValue(steamPlayerId, out var playerCooldowns) || playerCooldowns == null)
                        {
                            Cooldowns[steamPlayerId] = playerCooldowns = new Dictionary<string, DateTime>();
                        }
                        Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                    }
                }
            }
            // Si hay 2 argumentos -> dar el kit a otro jugador o a todos (*)
            else if (args.Length == 2)
            {
                if (!src.HasPermission($"essentials.kit.{kitName}.other"))
                {
                    return CommandResult.NoPermission($"essentials.kit.{kitName}.other");
                }

                if (!KItModule.Instance.KitManager.Contains(kitName))
                {
                    return CommandResult.LangError("icon_error_general", "KIT_NOT_EXIST", kitName);
                }

                var kit = KItModule.Instance.KitManager.GetByName(kitName);
                if (args[1].Equals("*"))
                {
                    if (player.IsAdmin || player.HasPermission("*"))
                    {
                        UServer.Players.ForEach(kit.GiveTo);
                        EssLang.Send("generalicon", src, "KIT_GIVEN_SENDER_ALL", messageaddon + kitName);
                    }
                    else
                    {
                        return CommandResult.LangError("icon_error_general", "KIT_NO_PERMISSION");
                    }
                }
                else
                {
                    if (!UPlayer.TryGet(args[1].ToString(), out var target))
                    {
                        return CommandResult.LangError("icon_error_general", "PLAYER_NOT_FOUND", args[1]);
                    }

                    if (!src.HasPermission("essentials.bypass.kitcooldown") && !src.IsConsole)
                    {
                        if (globalCooldown > 0) GlobalCooldown[steamPlayerId] = DateTime.Now;
                        if (kitCooldown > 0)
                        {
                            if (!Cooldowns.TryGetValue(steamPlayerId, out var playerCooldowns) || playerCooldowns == null)
                            {
                                Cooldowns[steamPlayerId] = playerCooldowns = new Dictionary<string, DateTime>();
                            }
                            Cooldowns[steamPlayerId][kitName] = DateTime.Now;
                        }
                    }

                    kit.GiveTo(target);
                    EssLang.Send("icon_kit_given-sender", src, "KIT_GIVEN_SENDER", messageaddon + kitName, target);
                }
            }

            return CommandResult.Success();
        }

    }
}