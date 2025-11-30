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

using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.CodeAnalysis;
using Essentials.I18n;
using Essentials.NativeModules.Kit.Item;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Essentials.NativeModules.Kit
{

    /// <summary>
    /// Author: leonardosnt
    /// </summary>
    public class Kit
    {

        /// <summary>
        /// Name of kit (obviously)
        /// </summary>
        public string Name { get; set; }

        public string Messageaddon { get; set; }
        /// <summary>
        /// List of items
        /// </summary>
        public List<AbstractKitItem> Items { get; set; }

        /// <summary>
        /// Cooldown in seconds
        /// </summary>
        public uint Cooldown { get; set; }

        /// <summary>
        /// Cost of kit.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Acceso al kit basado en reputación (puede ser negativo)
        /// </summary>
        public int Repuacces { get; set; }

        /// <summary>
        /// Flag that determines if cooldown will be reseted when player die
        /// </summary>
        public bool ResetCooldownWhenDie { get; set; }

        /// <summary>
        /// Constructor de Kit.
        /// </summary>
        /// <param name="name">Nombre del kit.</param>
        /// <param name="cooldown">Cooldown en segundos.</param>
        /// <param name="messagesaddon">Color del kit.</param>
        /// <param name="repuacces">nivel de reputacion  para conseguirse (permite valores negativos)</param>
        /// <param name="resetCooldownWhenDie">Indica si el cooldown se reinicia al morir.</param>

        public Kit([NotNull] string name, string messagesaddon, uint cooldown, bool resetCooldownWhenDie, int repuacces)
        {
            Name = name;
            Messageaddon = messagesaddon;
            Cooldown = cooldown;
            Repuacces = repuacces;
            ResetCooldownWhenDie = resetCooldownWhenDie;
            Items = new List<AbstractKitItem>();
        }

        public Kit(string name, string messagesaddon, uint cooldown, decimal cost, bool resetCooldownWhenDie, int repuacces)
            : this(name, messagesaddon, cooldown, resetCooldownWhenDie, repuacces)
        {
            Cost = cost;
        }

        /// <summary>
        /// <returns> Check if <paramref name="src"/>> has permission for this kit. </returns>
        /// </summary>
        public bool CanUse(ICommandSource src)
        {
            return src.HasPermission($"essentials.kit.{Name.ToLowerInvariant()}");
        }

        /// <summary>
        /// Give this kit to player
        /// </summary>
        public void GiveTo(UPlayer player)
        {
            var sentInventoryFullMessage = false;

            foreach (var kitItem in Items)
            {
                var added = kitItem.GiveTo(player);

                if (added || sentInventoryFullMessage) continue;

                EssLang.Send("icon_inventory_full", player, "INVENTORY_FULL");
                sentInventoryFullMessage = true;
            }
            // revisar futuros errores discord ellocoed
            EssLang.Send("icon_given_receiver", player, "KIT_GIVEN_RECEIVER", Messageaddon + Name);
        }

    }

}