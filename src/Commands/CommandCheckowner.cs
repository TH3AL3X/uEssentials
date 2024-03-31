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
using Essentials.Compatibility;
using Essentials.Components.Player;
using Essentials.Event.Handling;
using Essentials.I18n;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Framework.Utilities;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{

    [CommandInfo(
        Name = "checkowner",
        Description = "Checkowner of a structure,vehicle,barricade",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 0,
        MaxArgs = 0
    )]
    public class CommandCheckOwner : EssCommand
    {

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var look = player.Look;
            ulong owner = 0;

            if (PhysicsUtility.raycast(new Ray(look.aim.position, look.aim.forward), out RaycastHit hit, Mathf.Infinity,
                RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE))
            {
                var barricade = hit.transform.GetComponent<Interactable2SalvageBarricade>();
                var structure = hit.transform.GetComponent<Interactable2SalvageStructure>();
                var vehicle = hit.transform.GetComponent<InteractableVehicle>();

                if (structure != null)
                {
                    owner = structure.owner;
                }
                else if(barricade != null)
                {
                    owner = barricade.owner;
                }
                else if (vehicle != null)
                {
                    owner = vehicle.lockedOwner.m_SteamID;
                }

                EssLang.Send(src, "CHECKOWNER", owner);
            }
            else
            {
                return CommandResult.LangError("CHECKOWNER_LOOKING_NONE");
            }

            return CommandResult.Success();
        }
    }

}