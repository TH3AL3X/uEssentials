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
using HarmonyLib;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Reflection;
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

        // Unturned keeps acceleration in this velocity vector even when the
        // gravity multiplier is zero. PlayerMovement.move contains the
        // server-authoritative directional input, so we can stop only the
        // axes the player has released without interfering with WASD input.
        private static readonly FieldInfo MovementVelocityField =
            AccessTools.Field(typeof(PlayerMovement), "velocity");

        private const int MIN_INPUT_ARRAY_LENGTH = 12; // Minimum length for Unturned input keys array

        public void SetReady(UPlayer Player)
        {
            this.Player = Player.ToPlayer();
            UPlayer = this.Player;

            Ready = true;
            Player.Movement.sendPluginGravityMultiplier(Gravity);
            Player.Movement.sendPluginSpeedMultiplier(Speed);
            ResetMovementVelocity();
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
                if (Inputs.Length >= MIN_INPUT_ARRAY_LENGTH)
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

        // PlayerMovement performs its simulation in Update. LateUpdate runs
        // afterwards, so momentum is cleared before it can carry into the
        // next frame.
        private void LateUpdate()
        {
            if (awake && Ready)
            {
                StabilizeMovementVelocity();
            }
        }

        private void StabilizeMovementVelocity()
        {
            PlayerMovement movement = Player?.Movement;
            if (movement == null || MovementVelocityField == null)
            {
                return;
            }

            object rawVelocity = MovementVelocityField.GetValue(movement);
            if (!(rawVelocity is Vector3 velocity))
            {
                return;
            }

            bool hasHorizontalInput = HasHorizontalInput(movement);
            bool hasVerticalInput = !Mathf.Approximately(Gravity, 0f);

            if (!hasHorizontalInput)
            {
                velocity.x = 0f;
                velocity.z = 0f;
            }

            if (!hasVerticalInput)
            {
                velocity.y = 0f;
            }

            MovementVelocityField.SetValue(movement, velocity);
        }

        private static bool HasHorizontalInput(PlayerMovement movement)
        {
            Vector3 input = movement.move;
            return !Mathf.Approximately(input.x, 0f) ||
                   !Mathf.Approximately(input.z, 0f);
        }

        private void ResetMovementVelocity()
        {
            PlayerMovement movement = Player?.Movement;
            if (movement != null && MovementVelocityField != null)
            {
                MovementVelocityField.SetValue(movement, Vector3.zero);
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
            ResetMovementVelocity();
            Player.Movement.sendPluginGravityMultiplier(1);
            Player.Movement.sendPluginSpeedMultiplier(1);
        }

        public void OnDestroy()
        {
            Stop();
        }

    }

}
