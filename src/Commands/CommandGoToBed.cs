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

using System.Collections.Generic;
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
using Steamworks;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "gotobed",
        Usage = "[player]",
        Description = "Go to the bed of a player",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1,
        MaxArgs = 2
    )]
    public class CommandGoToBed : EssCommand
    {
        private List<(Vector3, byte)> GetBeds(UPlayer player)
        {
            var results = new List<(Vector3, byte)>();

            for (byte x = 0; x < BarricadeManager.regions.GetLength(0); x++)
            {
                for (byte y = 0; y < BarricadeManager.regions.GetLength(1); y++)
                {
                    var region = BarricadeManager.regions[x, y];

                    for (int i = 0; i < region.barricades.Count; i++)
                    {
                        var drop = region.drops[i];
                        var barricade = region.barricades[i];

                        if (drop?.interactable is InteractableBed bed && (CSteamID)barricade.owner == player.CSteamId)
                        {
                            results.Add((bed.transform.position, 0));
                        }
                    }
                }
            }

            return results;
        }
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (!args[0].IsValidPlayerIdentifier)
            {
                return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
            }

            var player = args[0].ToPlayer;

            var beds = GetBeds(player);

            if (beds.Count == 0)
                return CommandResult.LangError("GOTOBED_NOT_FOUND");

            if (beds.Count == 1)
            {
                src.ToPlayer().UnturnedPlayer.teleportToLocationUnsafe(beds[0].Item1, 0);
                EssLang.Send(src, "GOTOBED");
                return CommandResult.Success();
            }

            if (args.Length == 2 && int.TryParse(args[1].ToString(), out int index))
            {
                index -= 1;
                if (index >= 0 && index < beds.Count)
                {
                    var (loc, angle) = beds[index];
                    src.ToPlayer().UnturnedPlayer.teleportToLocationUnsafe(loc, 0);
                    EssLang.Send(src, "GOTOBED");
                    return CommandResult.Success();
                }
                else
                {
                    return CommandResult.LangError("GOTOBED_INVALID_INDEX");
                }
            }

            var bedList = string.Join(", ", beds.Select((b, i) => $"{i + 1}"));
            EssLang.Send(src, "GOTOBED_MULTIPLE", bedList, player.CSteamId);
            return CommandResult.Success();
        }
    }

}