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

namespace Essentials.NativeModules.Warp.Commands {

    [CommandInfo(
        Name = "setwarp",
        Description = "Set a warp.",
        Usage = "[warp_name] <x> <y> <z>"
    )]
    public class CommandSetWarp : EssCommand {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args) {
            switch (args.Length) {
                case 1:
                    if (src.IsConsole) {
                        return CommandResult.ShowUsage();
                    }

                    if (WarpModule.Instance.WarpManager.Contains(args[0].ToString())) {
                        return CommandResult.LangError("WARP_ALREADY_EXISTS");
                    }

                    var player = src.ToPlayer();
                    var warp = new Warp(args[0].ToString(), player.Position, player.Rotation);

                    WarpModule.Instance.WarpManager.Add(warp);
                    EssLang.Send(src, "WARP_SET", args[0]);
                    break;

                case 4:
                    var pos = args.GetVector3(1);

                    if (pos.HasValue) {
                        warp = new Warp(args[0].ToString(), pos.Value, 0.0F);

                        if (WarpModule.Instance.WarpManager.Contains(args[0].ToString())) {
                            return CommandResult.LangError("WARP_ALREADY_EXISTS");
                        }

                        WarpModule.Instance.WarpManager.Add(warp);

                        EssLang.Send(src, "WARP_SET", args[0]);
                    } else {
                        return CommandResult.LangError("INVALID_COORDS", args[1], args[2], args[3]);
                    }
                    break;

                default:
                    return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }

    }

}