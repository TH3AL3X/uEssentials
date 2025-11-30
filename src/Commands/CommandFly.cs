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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Components.Player;
using Essentials.I18n;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "fly",
        Description = "Fly like a bird",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 0,
        MaxArgs = 1
    )]
    public class CommandFly : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length > 0)
            {
                if (!src.HasPermission($"{Permission}.other"))
                {
                    return CommandResult.NoPermission($"{Permission}.other");
                }

                if (!UPlayer.TryGet(args[0].ToString(), out var player))
                {
                    return CommandResult.LangError("icon_error_general", "PLAYER_NOT_FOUND", args[0]);
                }

                var component_to_player = player.GetComponent<FlyPlayer>() ?? player.AddComponent<FlyPlayer>();

                if (component_to_player.session)
                {
                    EssLang.Send("generalicon", src, "FLY_TOPLAYER", "disabled", player.DisplayName);
                    player.RemoveComponent<FlyPlayer>();
                }
                else
                {
                    component_to_player.session = true;
                    EssLang.Send("generalicon", src, "FLY_TOPLAYER", "enabled", player.DisplayName);
                    component_to_player.SetReady(player);
                }
            }
            else
            {
                var player = src.ToPlayer();

                var component = player.GetComponent<FlyPlayer>() ?? player.AddComponent<FlyPlayer>();

                if (component.session)
                {
                    EssLang.Send("generalicon", src, "FLY", "disabled");
                    player.RemoveComponent<FlyPlayer>();
                }
                else
                {
                    component.session = true;
                    EssLang.Send("generalicon", src, "FLY", "enabled");
                    component.SetReady(player);
                }
            }
            return CommandResult.Success();
        }
    }


}