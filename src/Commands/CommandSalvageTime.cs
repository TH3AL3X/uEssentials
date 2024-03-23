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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.I18n;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "salvagetime",
        Aliases = new[] { "stime" },
        Description = "Change your salvage time. 0 = Freeze, 1 = Normal",
        Usage = "<amount>",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1,
        MaxArgs = 1
    )]
    public class CommandSalvageTime : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (!float.TryParse(args[0].ToString(), out var amount) || amount == 0f)
            {
                return CommandResult.LangError("INVALID_NUMBER", amount);
            }

            var player = src.ToPlayer();
<<<<<<< HEAD
            // not used
            // player.Movement.sendPluginSpeedMultiplier(amount);

            player.SteamPlayer.player.interact.sendSalvageTimeOverride(amount);
=======
            player.Movement.sendPluginSpeedMultiplier(amount);
>>>>>>> 265a67c35bab80a95b90e21dede132a5581f289a

            EssLang.Send(src, "SALVAGE_TIME_CHANGED", amount);
            return CommandResult.Success();
        }

    }

}