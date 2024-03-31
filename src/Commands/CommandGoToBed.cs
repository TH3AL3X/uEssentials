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

using System.Linq;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Components.Player;
using Essentials.Event.Handling;
using Essentials.I18n;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "gotobed",
        Usage = "[player]",
        Description = "Go to the bed of a player",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1,
        MaxArgs = 1
    )]
    public class CommandGoToBed : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length == 0)
            {
                return CommandResult.ShowUsage();
            }

            if (!UPlayer.TryGet(args[0].ToString(), out var player))
            {
                return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
            }

            if (BarricadeManager.tryGetBed(player.CSteamId, out var Location, out byte Angle))
            {
                src.ToPlayer().UnturnedPlayer.teleportToLocationUnsafe(Location, Angle);
                EssLang.Send(src, "GOTOBED");
            }
            else
            {
                return CommandResult.LangError("GOTOBED_NOT_FOUND");
            }

            return CommandResult.Success();
        }
    }

}