#region License
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

using SDG.Unturned;
using System.Globalization;
using System.Linq;
using Essentials.Api.Module;
using static Essentials.Api.UEssentials;
using Essentials.Api.Command;
using Essentials.Api;
using Essentials.I18n;

namespace Essentials.Common.Util {

    public static class VehicleUtil {
        private static readonly System.Collections.Generic.List<VehicleAsset> assets = new System.Collections.Generic.List<VehicleAsset>(); // avoid re-allocation per usage
        public static Asset GetVehicle(string name)
        {
            if (ushort.TryParse(name, out var id))
            {
                return Assets.find(EAssetType.VEHICLE, id);
            }
            else
            {
                // Updated obsolete list-fetch:
                if(assets.Count == 0) // only populate list once
                    Assets.find(assets); // assets normally do not update at runtime, except during server start (i.e. after downloading workshop)

                for (int i = 0; i < assets.Count; i++) // faster than foreach (especially until dotnet 10)
                {
                    var ia = assets[i];
                    if (ia?.FriendlyName == null)
                        continue;

                    if (ia.FriendlyName.IndexOf(name, System.StringComparison.OrdinalIgnoreCase) >= 0) // avoid .ToLower() which allocates each iteration
                        return ia; // found the VehicleAsset here
                }

                return null;
            }
        }
    }

}