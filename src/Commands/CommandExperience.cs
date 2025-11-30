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
using Essentials.Common;
using Essentials.I18n;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "experience",
        Aliases = new[] { "exp" },
        Description = "Give experience to you/player",
        Usage = "[amount] <target/*>"
    )]
    public class CommandExperience : EssCommand
    {

        private const int MAX_INPUT_VALUE = 10000000;

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.Length == 0 || (args.Length == 1 && src.IsConsole))
            {
                return CommandResult.ShowUsage();
            }

            if (!args[0].IsInt)
            {
                return CommandResult.LangError("icon_error_general", "INVALID_NUMBER", args[0]);
            }

            var amount = args[0].ToInt;

            if (amount > MAX_INPUT_VALUE || amount < -MAX_INPUT_VALUE)
            {
                return CommandResult.LangError("icon_error_general", "NUMBER_BETWEEN", -MAX_INPUT_VALUE, MAX_INPUT_VALUE);
            }

            if (args.Length == 2)
            {
                // Everyone
                if (args[1].Equals("*"))
                {
                    if (!src.HasPermission($"{Permission}.all"))
                    {
                        return CommandResult.NoPermission($"{Permission}.all");
                    }

                    UServer.Players.ForEach(p => GiveExp(p, amount));

                    if (amount >= 0)
                    {
                        EssLang.Send("iconexperience_given", src, "EXPERIENCE_GIVEN", amount, EssLang.Translate("EVERYONE").ToLowerInvariant());
                    }
                    else
                    {
                        EssLang.Send("iconexperience_take", src, "EXPERIENCE_TAKE", -amount, EssLang.Translate("EVERYONE").ToLowerInvariant());
                    }
                }
                else if (!args[1].IsValidPlayerIdentifier)
                {
                    return CommandResult.LangError("icon_error_general", "PLAYER_NOT_FOUND", args[1]);
                }
                else
                { // Other player
                    if (!src.HasPermission($"{Permission}.other"))
                    {
                        return CommandResult.NoPermission($"{Permission}.other");
                    }

                    var player = args[1].ToPlayer;

                    if (amount >= 0)
                    {
                        EssLang.Send("iconexperience_given", src, "EXPERIENCE_GIVEN", amount, player.DisplayName.ToLowerInvariant());
                    }
                    else
                    {
                        EssLang.Send("iconexperience_take", src, "EXPERIENCE_TAKE", -amount, player.DisplayName.ToLowerInvariant());
                    }

                    GiveExp(player, amount);
                }
            }
            else
            {
                GiveExp(src.ToPlayer(), amount);
            }

            return CommandResult.Success();
        }

        private void GiveExp(UPlayer player, int amount)
        {
            if (amount >= 0)
            {
                EssLang.Send("iconexperience_given", player, "EXPERIENCE_RECEIVED", amount, player.DisplayName.ToLowerInvariant());
            }
            else
            {
                EssLang.Send("iconexperience_take", player, "EXPERIENCE_LOST", -amount, player.DisplayName.ToLowerInvariant());
            }

            player.UnturnedPlayer.skills.askAward((uint)amount);
        }

    }

}