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
using Essentials.Api.Command.Source;
using Essentials.CodeAnalysis;
using Essentials.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace Essentials.NativeModules.Warp {

    /// <summary>
    /// Author: leonardosnt
    /// </summary>
    public sealed class Warp {

        /// <summary>
        /// Name of warp
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Rotation of warp
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Location in the world of warp
        /// </summary>
        [JsonConverter(typeof(Vector3Converter))]
        public Vector3 Location { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name">Name of warp</param>
        /// <param name="location">Location of warp</param>
        /// <param name="rotation">Rotation of warp</param>
        public Warp([NotNull] string name, Vector3 location, float rotation) {
            Name = name;
            Rotation = rotation;
            Location = location;
        }

        /// <summary>
        /// Check If source has permission to use this warp
        /// </summary>
        /// <param name="source">Source that you want to check if is authorized</param>
        /// <returns>If source has permission to use this warp</returns>
        public bool CanBeUsedBy(ICommandSource source) {
            return source.HasPermission($"essentials.warp.{Name.ToLowerInvariant()}") || !UEssentials.Config.Warp.PerWarpPermission;
        }

        public override string ToString() {
            return "Warp{Name= " + Name + ", " +
                   "Location= " + Location + ", " +
                   "Rotation= " + Rotation + "}";
        }

    }

}