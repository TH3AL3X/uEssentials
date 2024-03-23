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
using UnityEngine;

namespace Essentials.Api.Command {

    public static class CommandExtensions {

        /// <summary>
        /// Try to parse a Vector3 from 3 arguments, starting in <paramref name="initialIndex"/>
        /// </summary>
        /// <param name="src">Source</param>
        /// <param name="initialIndex"> Initial index </param>
        /// <returns>New vector3 with given positions.</returns>
        public static Vector3? GetVector3(this ICommandArgs src, int initialIndex) {
            if (initialIndex + 3 > src.Length) {
                return null;
            }

            var x = src.Arguments[initialIndex];
            var y = src.Arguments[initialIndex + 1];
            var z = src.Arguments[initialIndex + 2];

            if (x.IsFloat && y.IsFloat && z.IsFloat) {
                return new Vector3(x.ToFloat, y.ToFloat, z.ToFloat);
            }

            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="minInclusive">Maximum value</param>
        /// <param name="maxInclusive">Minimum value</param>
        /// <returns></returns>
        public static bool IsInRange(this ICommandArgument src, int minInclusive, int maxInclusive) {
            var val = src.ToInt;
            return val >= minInclusive && val <= maxInclusive;
        }

        /// <summary>
        ///  Try to convert the argument to byte.
        /// </summary>
        /// <param name="value">the converted value</param>
        /// <param name="error">
        ///  CommandResult.LangError("NUMBER_BETWEEN", byte.MinValue, byte.MaxValue) if out of range;
        //   CommandResult.LangError("INVALID_NUMBER", src.ToString()) if invalid.
        /// </param>
        /// <returns>true if sucessfull, otherwise false</returns>
        public static bool TryConvertToByte(this ICommandArgument src, out byte value, out CommandResult error) {
            value = 0;
            error = null;
            try {
                value = byte.Parse(src.ToString());
                return true;
            } catch (OverflowException) {
                error = CommandResult.LangError("NUMBER_BETWEEN", byte.MinValue, byte.MaxValue);
            } catch (FormatException) {
                error = CommandResult.LangError("INVALID_NUMBER", src.ToString());
            }
            return false;
        }

        public static bool TryConvertToByte(this ICommandArgument src, out byte? value, out CommandResult error) {
            byte result;
            if (src.TryConvertToByte(out result, out error)) {
                value = result;
                return true;
            }
            value = null;
            return false;
        }
    }

}