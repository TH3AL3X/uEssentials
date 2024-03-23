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

namespace Essentials.Common.Util {

    public static class VehicleUtil {

        private static IOrderedEnumerable<VehicleAsset> _cachedAssets;

        public static Optional<VehicleAsset> GetVehicle(ushort id) {
            return Optional<VehicleAsset>.OfNullable((VehicleAsset) Assets.find(EAssetType.VEHICLE, id));
        }

        public static Optional<VehicleAsset> GetVehicle(string name) {
            if (name == null) {
                return Optional<VehicleAsset>.Empty();
            }

            if (ushort.TryParse(name, out var id)) {
                return GetVehicle(id);
            }

            if (_cachedAssets == null) {
                _cachedAssets = Assets.find(EAssetType.VEHICLE)
                    .Cast<VehicleAsset>()
                    .Where(i => i.vehicleName != null)
                    .OrderBy(i => i.id);
            }

            var lastAsset = null as VehicleAsset;
            var lastPriority = 0;

            foreach (var asset in _cachedAssets) {
                var itemPriority = 0;
                var vehName = asset.vehicleName;

                if (vehName.EqualsIgnoreCase(name)) {
                    lastAsset = asset;
                    break;
                }

                if (vehName.StartsWith(name, true, CultureInfo.InvariantCulture)) {
                    itemPriority = 3;
                } else if (vehName.ContainsIgnoreCase(name)) {
                    itemPriority = 2;
                } else if (name.IndexOf(' ') > 0 && name.Split(' ').All(p => vehName.ContainsIgnoreCase(p))) {
                    itemPriority = 1;
                }

                if (itemPriority > lastPriority) {
                    lastAsset = asset;
                    lastPriority = itemPriority;
                }
            }

            return Optional<VehicleAsset>.OfNullable(lastAsset);
        }

    }

}