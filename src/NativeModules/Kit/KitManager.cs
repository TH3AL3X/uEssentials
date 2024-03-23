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
using Essentials.NativeModules.Kit.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace Essentials.NativeModules.Kit {

    public sealed class KitManager {

        internal IData<Dictionary<string, Kit>> KitData { get; }
        internal CooldownData CooldownData { get; }
        private Dictionary<string, Kit> KitMap { get; set; }
        public IEnumerable<Kit> Kits => KitMap.Values;
        public int Count => KitMap.Count;

        internal KitManager() {
            CooldownData = new CooldownData();
            KitMap = new Dictionary<string, Kit>();
            KitData = new KitData();

            var webResources = UEssentials.Core.WebResources;
            if (webResources.Loaded.ContainsKey("Kits")) {
                File.WriteAllText(Data.KitData.DataFilePath, webResources.Loaded["Kits"]);
            }
        }

        public bool Contains(string kitName) {
            return KitMap.ContainsKey(kitName.ToLowerInvariant());
        }

        public bool Contains(Kit kit) {
            return KitMap.ContainsValue(kit);
        }

        public void Add(Kit kit) {
            KitMap.Add(kit.Name.ToLowerInvariant(), kit);
            KitData.Save(KitMap);
        }

        public void Remove(Kit kit) {
            KitMap.Remove(kit.Name.ToLowerInvariant());
            KitData.Save(KitMap);
        }

        public Kit GetByName(string kitName) {
            return Contains(kitName)
                ? KitMap[kitName.ToLowerInvariant()]
                : null;
        }

        public void LoadKits() {
            KitMap = KitData.Load();
        }

        public void SaveKits() {
            KitData.Save(KitMap);
        }

        public void Load() {
            try {
                LoadKits();
                CooldownData.Load();
            } catch (Exception ex) {
                UEssentials.Logger.LogError("An error ocurred while loading kits...");
                UEssentials.Logger.LogException(ex);
            }
        }

        public void Save() {
            Console.WriteLine("kits saved");
            try {
                SaveKits();
                CooldownData.Save();
            } catch (Exception ex) {
                UEssentials.Logger.LogError("An error ocurred while saving kits...");
                UEssentials.Logger.LogException(ex);
            }
        }

    }

}