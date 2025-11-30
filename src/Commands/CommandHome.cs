#region License
/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2024 Terror
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
using System;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "home",
        Aliases = new[] { "h" },
        Description = "Teleport to your bed.",
        AllowedSource = AllowedSource.PLAYER
    )]
    public class CommandHome : EssCommand
    {

        internal static PlayerDictionary<Task> Delay = new PlayerDictionary<Task>(
            PlayerDictionaryOptions.LAZY_REGISTER_HANDLERS |
            PlayerDictionaryOptions.REMOVE_ON_DEATH |
            PlayerDictionaryOptions.REMOVE_ON_DISCONNECT,
            task => task.Cancel()
        );

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var playerId = player.CSteamId;
            //corregir futuros errores y optimizar  discord ellocoed
            if (player.Stance == EPlayerStance.DRIVING ||
                player.Stance == EPlayerStance.SITTING)
            {
                return CommandResult.LangError("icon_error_general", "CANNOT_TELEPORT_IN_VEHICLE");
            }

            if (!BarricadeManager.tryGetBed(player.CSteamId, out var bedPosition, out var bedAngle))
            {
                return CommandResult.LangError("icon_error_general", "WITHOUT_BED");
            }

            if (Delay.ContainsKey(player.CSteamId.m_SteamID))
            {
                return CommandResult.LangError("icon_error_general", "ALREADY_WAITING");
            }

            var homeCommand = UEssentials.Config.Home;
            var delay = homeCommand.TeleportDelay;

            if (player.HasPermission("essentials.bypass.homecooldown"))
            {
                delay = 0;
            }

            if (delay > 0)
            {
                EssLang.Send("generalicon", src, "TELEPORT_DELAY", TimeUtil.FormatSeconds((uint)delay));
            }

            var task = Task.Create()
                   .Delay(TimeSpan.FromSeconds(delay))
                   .Action(t =>
                   {
                       Delay.Remove(playerId.m_SteamID);
                       player.Teleport(bedPosition + new Vector3(0f, 0.5f, 0f), bedAngle);
                       EssLang.Send("generalicon", src, "TELEPORTED_BED");
                   })
                   .Submit();

            Delay.Add(playerId.m_SteamID, task);

            return CommandResult.Success();
        }

        protected override void OnUnregistered()
        {
            Delay.Clear();
            UEssentials.EventManager.Unregister<EssentialsEventHandler>("HomePlayerMove");
        }

    }

}
