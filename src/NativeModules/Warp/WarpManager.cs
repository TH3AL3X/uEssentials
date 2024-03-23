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

using Essentials.Api;
using Essentials.Core.Storage;
using Essentials.NativeModules.Warp.Data;
using System;
using System.Collections.Generic;

namespace Essentials.NativeModules.Warp {

    public sealed class WarpManager {

        private Dictionary<string, Warp> WarpMap { get; set; }
        private IData<Dictionary<string, Warp>> WarpData { get; }

        public IEnumerable<Warp> Warps => WarpMap.Values;
        public int Count => WarpMap.Count;

        internal WarpManager() {
            WarpMap = new Dictionary<string, Warp>();
            WarpData = new WarpData();
        }

        public bool Contains(string warpName) {
            return WarpMap.ContainsKey(warpName.ToLowerInvariant());
        }

        public bool Contains(Warp warp) {
            return WarpMap.ContainsValue(warp);
        }

        public void Load() {
            try {
                WarpMap = WarpData.Load();
            } catch (Exception ex) {
                UEssentials.Logger.LogError("An error ocurred while loading warps...");
                UEssentials.Logger.LogException(ex);
            }
        }

        public void Save() {
            try {
                WarpData.Save(WarpMap);
            } catch (Exception ex) {
                UEssentials.Logger.LogError("An error ocurred while saving warps...");
                UEssentials.Logger.LogException(ex);
            }
        }

        public void Add(Warp warp) {
            WarpMap.Add(warp.Name.ToLowerInvariant(), warp);
            Save();
        }

        public Warp GetByName(string warpName) {
            return Contains(warpName)
                ? WarpMap[warpName.ToLowerInvariant()]
                : null;
        }

        public bool Delete(string warpName) {
            var success = WarpMap.Remove(warpName.ToLowerInvariant());
            Save();
            return success;
        }

        public void Delete(Warp warp) {
            Delete(warp.Name);
        }

    }

}