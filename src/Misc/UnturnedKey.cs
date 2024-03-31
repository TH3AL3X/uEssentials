using System;
using System.Collections.Generic;
using System.Text;

namespace Essentials.src.Misc
{
    public enum UnturnedKey : int
    {
        Unknown = -1,
        Jump = 0,
        LPunch = 1,
        RPunch = 2,
        /// <summary>
        /// WARNING: Key status will be down while the player is crouched, even if they are not holding the key down.
        /// </summary>
        Crouch = 3,
        /// <summary>
        /// WARNING: Key status will be down while the player is prone, even if they are not holding the key down.
        /// </summary>
        Prone = 4,
        Sprint = 5,
        Leanleft = 6,
        LeanRight = 7,
        /// <summary>
        /// Defaults to comma
        /// </summary>
        HoldBreath = 9,
        CodeHotkey1 = 10,
        /// <summary>
        /// Defaults to period
        /// </summary>
        CodeHotkey2 = 11,
        /// <summary>
        /// Defaults to forward slash
        /// </summary>
        CodeHotkey3 = 12,
        /// <summary>
        /// Defaults to semicolon
        /// </summary>
        CodeHotkey4 = 13,
    }
}
