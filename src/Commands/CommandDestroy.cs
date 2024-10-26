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
using Essentials.I18n;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "destroy",
        Description = "Destroys the barricade or structure that you are looking at.",
        AllowedSource = AllowedSource.PLAYER,
        MinArgs = 0,
        MaxArgs = 0
    )]
    public class CommandDestroy : EssCommand
    {
        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            var player = src.ToPlayer();
            var look = player.Look;


            if (Physics.Raycast(look.aim.position, look.aim.forward, out var hit, Mathf.Infinity, RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.VEHICLE))
            {
                var veh = hit.transform.GetComponent<InteractableVehicle>();

                switch (hit.transform.tag)
                {
                    case "Barricade":
                        if (BarricadeManager.tryGetRegion(hit.transform, out var x, out var y, out var num, out var barricadeRegion))
                        {
                            var barricadeDrop = barricadeRegion?.FindBarricadeByRootTransform(hit.transform);

                            if (barricadeDrop != null)
                            {
                                BarricadeManager.destroyBarricade(barricadeDrop, x, y, num);
                                EssLang.Send(src, "BARRICADE_REMOVED");
                                return CommandResult.Success();
                            }
                            else
                            {
                                /* Case for doors */
                                barricadeDrop = barricadeRegion.FindBarricadeByRootTransform(hit.transform.root);
                                BarricadeManager.destroyBarricade(barricadeDrop, x, y, num);
                                EssLang.Send(src, "BARRICADE_REMOVED");
                                return CommandResult.Success();
                            }
                        }
                        break;

                    case "Structure":
                        if (StructureManager.tryGetRegion(hit.transform, out x, out y, out var structureRegion))
                        {
                            var structureDrop = structureRegion?.FindStructureByRootTransform(hit.transform);
                            if (structureDrop != null)
                            {
                                StructureManager.destroyStructure(structureDrop, x, y, Vector3.zero);
                                EssLang.Send(src, "STRUCTURE_REMOVED");
                                return CommandResult.Success();
                            }
                        }
                        break;

                    case "Vehicle":
                        if (veh != null)
                        {
                            VehicleManager.askVehicleDestroy(veh);
                            EssLang.Send(src, "VEHICLE_REMOVED");
                            return CommandResult.Success();
                        }
                        break;
                }
            }

            return CommandResult.LangError("DESTROY_INVALID");
        }
    }
}