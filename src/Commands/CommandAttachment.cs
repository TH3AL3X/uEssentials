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

using System;
using System.Linq;
using System.Numerics;
using Essentials.Api;
using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.Compatibility;
using Essentials.Components.Player;
using Essentials.Event.Handling;
using Essentials.I18n;
using HarmonyLib;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "setattachment",
        Usage = "[attachment name/id]",
        Description = "Add attachment to a weapon",
        Aliases = new[] { "sments" },
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 1,
        MaxArgs = 1
    )]
    // Another copy from shimmytools, i only changed the if else if... to a switch, idk what he doing
    public class CommandAttachment : EssCommand
    {
        readonly static EItemType[] Types = new EItemType[] { EItemType.BARREL, EItemType.GRIP, EItemType.MAGAZINE, EItemType.SIGHT, EItemType.TACTICAL };
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var Player = src.ToPlayer();

            if (args.Length > 0)
            {
                if (Player.Equipment.state == null || Player.Equipment.state.Length < 12 || Player.Equipment.asset == null || Player.Equipment.asset.type != EItemType.GUN)
                    return CommandResult.LangError("SET_ATTACHMENT_FAIL");

                ItemAsset Item = null;

                if (ushort.TryParse(args[0].ToString(), out ushort ItemID))
                {
                    Asset SelectAsset = Assets.find(EAssetType.ITEM, ItemID);
                    if (SelectAsset != null && typeof(ItemAsset).IsAssignableFrom(SelectAsset.GetType()) && Types.Contains(((ItemAsset)SelectAsset).type))
                    {
                        Item = (ItemAsset)SelectAsset;
                    }
                }

                if (Item == null)
                {
                    ItemAsset[] Ast = Assets.find(EAssetType.ITEM).Where(x => typeof(ItemAsset).IsAssignableFrom(x.GetType()) &&
                        Types.Contains(((ItemAsset)x).type) &&
                        ((ItemAsset)x).itemName.ToLower().Contains(args[0].ToString().ToLower()))
                        .Cast<ItemAsset>()
                        .ToArray();
                    if (Ast.Length != 0) Item = Ast[0];
                }

                if (Item != null)
                {
                    byte pos = 255;

                    switch (Item.type)
                    {
                        case EItemType.SIGHT:
                            pos = 0;
                            break;
                        case EItemType.TACTICAL:
                            pos = 2;
                            break;
                        case EItemType.GRIP:
                            pos = 4;
                            break;
                        case EItemType.BARREL:
                            pos = 6;
                            break;
                        case EItemType.MAGAZINE:
                            pos = 8;
                            break;
                    }

                    if (pos == 255)
                        return CommandResult.LangError("SET_ATTACHMENT_FAIL");

                    byte[] ID = BitConverter.GetBytes(Item.id);

                    Array.Copy(ID, 0, Player.Equipment.state, pos, 2);

                    Player.Equipment.sendUpdateState();
                    EssLang.Send(src, "SET_ATTACHMENT");
                }
                else
                {
                    return CommandResult.LangError("SET_ATTACHMENT_FAIL");
                }
            }
            else
            {
                return CommandResult.ShowUsage();
            }

            return CommandResult.Success();
        }
    }
}