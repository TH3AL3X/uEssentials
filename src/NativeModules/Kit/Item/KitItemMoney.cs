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

using Essentials.Api;
using Essentials.Api.Unturned;
using Newtonsoft.Json;

namespace Essentials.NativeModules.Kit.Item {

    public class KitItemMoney : AbstractKitItem {

        [JsonProperty("Money")]
        public decimal Amount { get; set; }

        public KitItemMoney(decimal amount) {
            Amount = amount;
        }

        public override bool GiveTo(UPlayer player, bool dropIfInventoryFull = true) {
            UEssentials.EconomyProvider.IfPresent(h => h.Deposit(player, Amount));
            return true;
        }

        public override string ToString() {
            return $"Money: {Amount}";
        }

    }

}