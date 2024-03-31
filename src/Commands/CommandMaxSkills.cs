﻿#region License

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
using Essentials.I18n;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "maxskills",
        Description = "Set to max level all of your/player skills",
        Usage = "<[true|false]> <player | *>"
    )]
    public class CommandMaxSkills : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.IsEmpty)
            {
                if (src.IsConsole)
                {
                    return CommandResult.ShowUsage();
                }

                GiveMaxSkills(src.ToPlayer(), false);
            }
            else
            {
                bool overpower = args[0].ToBool;
                if (args.Length < 2 && src.IsConsole)
                {
                    return CommandResult.ShowUsage();
                }

                if (args.Length < 2 && !src.IsConsole)
                {
                    GiveMaxSkills(src.ToPlayer(), overpower);
                    return CommandResult.Success();
                }
                
                // player or all
                if (args.Length > 1)
                {
                    if (args[1].Equals("*"))
                    {
                        if (!src.HasPermission($"{Permission}.all"))
                        {
                            return CommandResult.NoPermission($"{Permission}.all");
                        }

                        // idk why i changed this, anyways is working better i think
                        foreach (SteamPlayer sPlayer in Provider.clients)
                        {
                            GiveMaxSkills(UPlayer.From(sPlayer), overpower);
                        }

                        EssLang.Send(src, "MAX_SKILLS_ALL");
                    }
                    else
                    {
                        if (!src.HasPermission($"{Permission}.other"))
                        {
                            return CommandResult.NoPermission($"{Permission}.other");
                        }

                        if (!args[1].IsValidPlayerIdentifier)
                        {
                            return CommandResult.LangError("PLAYER_NOT_FOUND", args[1]);
                        }

                        var targetPlayer = args[1].ToPlayer;
                        GiveMaxSkills(targetPlayer, overpower);
                        EssLang.Send(src, "MAX_SKILLS_TARGET", targetPlayer.DisplayName);
                    }
                }
            }

            return CommandResult.Success();
        }

        private void GiveMaxSkills(UPlayer player, bool overpower)
        {
            switch (overpower)
            {
                case true:
                    foreach (var skills in player.UnturnedPlayer.skills.skills)
                    {
                        foreach (var skill in skills)
                        {
                            skill.maxUnlockableLevel = byte.MaxValue;
                            skill.max = byte.MaxValue;
                        }
                    }
                    player.UnturnedPlayer.skills.ServerUnlockAllSkills();
                    break;
                case false:
                    for (var i = 0; i < player.UnturnedPlayer.skills.skills.Length; i++)
                    {
                        for (var j = 0; j < player.UnturnedPlayer.skills.skills[i].Length; j++)
                        {
                            var uSkill = USkill.Skills.First(x => x.SpecialityIndex == i && x.SkillIndex == j);
                            player.UnturnedPlayer.skills.skills[i][j].max = uSkill.Max;
                        }
                    }
                    player.UnturnedPlayer.skills.ServerUnlockAllSkills();
                    break;
            }

            EssLang.Send(player, "MAX_SKILLS");
        }
    }
}