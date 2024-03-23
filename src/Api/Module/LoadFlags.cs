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

using System;

namespace Essentials.Api.Module {

    [Flags]
    public enum LoadFlags {

        /// <summary>
        /// Nothing
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Dynamically register all commands in Module.
        /// </summary>
        AUTO_REGISTER_COMMANDS = 1,

        /// <summary>
        /// Dynamically register all events (handlers) in Module.
        /// </summary>
        AUTO_REGISTER_EVENTS = 1 << 1

    }

}