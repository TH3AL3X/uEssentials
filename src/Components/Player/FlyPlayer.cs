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
using Essentials.src.Misc;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using UnityEngine;

// This component was copied from ShimmyTools...
namespace Essentials.Components.Player
{
    public class FlyPlayer : MonoBehaviour
    {
        private readonly Dictionary<int, bool> KeyIndex = new Dictionary<int, bool>();
        public bool awake = false;

        public bool session = false;

        private bool Ready = false;

        public UPlayer Player;
        public UPlayer UPlayer;

        public float VerticalSpeed = 1;
        public float Gravity = 0;
        public float Speed = 1;

        private bool IsDescending = false;

        public void SendUpdateSpeed() => NeedUpdateSpeed = true;

        private bool NeedUpdateSpeed = false;

        public void SendUpdateGravity() => NeedUpdateGravity = true;

        private bool NeedUpdateGravity = false;

        public void SetReady(UPlayer Player)
        {
            this.Player = Player.ToPlayer();
            UPlayer = this.Player;

            Ready = true;
            Player.Movement.sendPluginGravityMultiplier(Gravity);
            Player.Movement.sendPluginSpeedMultiplier(Speed);
        }

        public void Awake()
        {
            awake = true;
        }

        private void OnKeyStateChanged(UnturnedKey Key, bool State)
        {
            if (Key == UnturnedKey.Jump)
            {
                if (State)
                {
                    Gravity = VerticalSpeed * -1;
                    Player.Movement.sendPluginGravityMultiplier(Gravity);
                }
                else
                {
                    Gravity = 0;
                    Player.Movement.sendPluginGravityMultiplier(Gravity);
                }
            }
            else if (Key == UnturnedKey.Sprint)
            {
                if (State)
                {
                    if (Player.Look.pitch > 160)
                    {
                        Gravity = VerticalSpeed;
                        IsDescending = true;
                        Player.Movement.sendPluginGravityMultiplier(Gravity);
                    }
                }
                else
                {
                    if (IsDescending)
                    {
                        IsDescending = false;
                        Gravity = 0;
                        Player.Movement.sendPluginGravityMultiplier(Gravity);
                    }
                }
            }
            else if (Key == UnturnedKey.CodeHotkey1)
            {
                if (State)
                {
                    Speed -= 1;
                    Player.Movement.sendPluginSpeedMultiplier(Speed);
                }
            }
            else if (Key == UnturnedKey.CodeHotkey2)
            {
                if (State)
                {
                    Speed -= 1;
                    Player.Movement.sendPluginSpeedMultiplier(Speed);
                }
            }
            else if (Key == UnturnedKey.CodeHotkey3)
            {
                if (State)
                {
                    Player.Movement.sendPluginSpeedMultiplier(Speed);
                    Player.Movement.sendPluginGravityMultiplier(Gravity);
                }
            }
        }

        private void CheckState(UnturnedKey Key, bool[] Inputs)
        {
            bool State = Inputs[(int)Key];
            if (CheckChanged((int)Key, State))
            {
                OnKeyStateChanged(Key, State);
            }
        }

        private bool CheckChanged(int Index, bool State)
        {
            if (KeyIndex.ContainsKey(Index))
            {
                bool LastState = KeyIndex[Index];
                if (LastState != State)
                {
                    KeyIndex[Index] = State;
                    return true;
                }
            }
            else
            {
                KeyIndex.Add(Index, State);
            }
            return false;
        }

        public void FixedUpdate()
        {
            if (awake && Ready)
            {
                bool[] Inputs = Player.UnturnedPlayer.input.keys;
                if (Inputs.Length >= 12)
                {
                    CheckState(UnturnedKey.Jump, Inputs);
                    CheckState(UnturnedKey.Sprint, Inputs);
                    CheckState(UnturnedKey.CodeHotkey1, Inputs);
                    CheckState(UnturnedKey.CodeHotkey2, Inputs);
                    CheckState(UnturnedKey.CodeHotkey3, Inputs);
                }
                CheckNeeds();
            }
        }

        private void CheckNeeds()
        {
            if (NeedUpdateSpeed)
            {
                NeedUpdateSpeed = false;
                Player.Movement.sendPluginSpeedMultiplier(Speed);
            }

            if (NeedUpdateGravity)
            {
                NeedUpdateGravity = false;
                Player.Movement.sendPluginGravityMultiplier(Gravity);
            }
        }

        public void Stop()
        {
            awake = false;
            Player.Movement.sendPluginGravityMultiplier(1);
            Player.Movement.sendPluginSpeedMultiplier(1);
        }

        public void OnDestroy()
        {
            Stop();
        }

    }

}