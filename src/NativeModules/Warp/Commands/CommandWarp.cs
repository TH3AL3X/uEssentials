﻿#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2018  leonardosnt
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/
#endregion

using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Task;
using Essentials.Common.Util;
using Essentials.Event.Handling;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.NativeModules.Warp.Commands {

    [CommandInfo(
        Name = "warp",
        Description = "Teleport you to given warp.",
        AllowedSource = AllowedSource.PLAYER,
        Usage = "[warp_name]"
    )]
    public class CommandWarp : EssCommand {

        internal static PlayerDictionary<Task> Delay = new PlayerDictionary<Task>(
            PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS |
            PlayerDictionaryOptions.REMOVE_ON_DEATH |
            PlayerDictionaryOptions.REMOVE_ON_DISCONNECT,
            task => task.Cancel()
        );

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            var player = src.ToPlayer();

            if (args.Length == 0 || args.Length > 1) {
                return CommandResult.ShowUsage();
            }

            if (!WarpModule.Instance.WarpManager.Contains(args[0].ToString())) {
                return CommandResult.LangError("WARP_NOT_EXIST", args[0]);
            }

            if (player.Stance == EPlayerStance.DRIVING ||
                player.Stance == EPlayerStance.SITTING) {
                return CommandResult.LangError("CANNOT_TELEPORT_DRIVING");
            }

            if (Delay.ContainsKey(player.CSteamId.m_SteamID)) {
                return CommandResult.LangError("ALREADY_WAITING");
            }

            var targetWarp = WarpModule.Instance.WarpManager.GetByName(args[0].ToString());
            var cooldown = UEssentials.Config.Warp.TeleportDelay;

            if (!targetWarp.CanBeUsedBy(src)) {
                return CommandResult.LangError("WARP_NO_PERMISSION", args[0]);
            }

            if (cooldown > 0 && !player.HasPermission("essentials.bypass.warpcooldown")) {
                EssLang.Send(src, "WARP_COOLDOWN", cooldown);
            }

            var task = Task.Create()
                .Id($"Warp teleport '{player.DisplayName}'")
                .Delay(player.HasPermission("essentials.bypass.warpcooldown") ? 0 : cooldown * 1000)
                .Action(t => {
                    Delay.Remove(player.CSteamId.m_SteamID);
                    player.Teleport(targetWarp.Location, targetWarp.Rotation);
                    EssLang.Send(src, "WARP_TELEPORTED", args[0]);
                })
                .Submit();

            Delay.Add(player.CSteamId.m_SteamID, task);

            return CommandResult.Success();
        }

        protected override void OnUnregistered() {
            Delay.Clear();
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("WarpPlayerMove");
        }

    }

}