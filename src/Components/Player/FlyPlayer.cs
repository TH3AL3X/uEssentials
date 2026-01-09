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

        // Drift fix: Track position for drift correction
        private Vector3 _lastPosition;
        private Vector3 _targetPosition;
        private bool _isMovementInputActive = false;
        private int _driftCorrectionFrames = 0;
        private const float DRIFT_THRESHOLD = 0.01f;
        private const float DRIFT_CORRECTION_STRENGTH = 0.3f;
        private const float HORIZONTAL_MOVEMENT_THRESHOLD = 0.1f;
        private const int MIN_INPUT_ARRAY_LENGTH = 12; // Minimum length for Unturned input keys array
        private const int MIN_DRIFT_FRAMES_BEFORE_CORRECTION = 1; // Frames to wait before applying correction

        public void SetReady(UPlayer Player)
        {
            this.Player = Player.ToPlayer();
            UPlayer = this.Player;

            Ready = true;
            Player.Movement.sendPluginGravityMultiplier(Gravity);
            Player.Movement.sendPluginSpeedMultiplier(Speed);
            
            // Drift fix: Initialize position tracking
            _lastPosition = Player.Position;
            _targetPosition = Player.Position;
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
                    
                    // Drift fix: Check if player is actively moving
                    _isMovementInputActive = IsPlayerMoving(Inputs);
                }
                
                CheckNeeds();
                
                // Drift fix: Apply drift correction
                ApplyDriftCorrection();
            }
        }

        // Drift fix: Check if player has active movement input
        private bool IsPlayerMoving(bool[] inputs)
        {
            if (inputs.Length < MIN_INPUT_ARRAY_LENGTH) return false;
            
            // Check for vertical movement keys which we track
            bool jump = inputs[(int)UnturnedKey.Jump];
            bool sprint = inputs[(int)UnturnedKey.Sprint];
            
            // For horizontal movement, we'll detect it by position change magnitude
            // This is handled in ApplyDriftCorrection
            return jump || (sprint && IsDescending);
        }

        // Drift fix: Apply velocity damping and position correction
        private void ApplyDriftCorrection()
        {
            if (Player == null || Player.UnturnedPlayer == null) return;
            
            Vector3 currentPos = Player.Position;
            
            // Calculate horizontal movement
            Vector3 horizontalMovement = currentPos - _lastPosition;
            horizontalMovement.y = 0; // Only care about horizontal drift
            float movementMagnitude = horizontalMovement.magnitude;
            
            // Detect if player is actively moving horizontally (large movement = intentional)
            // Small movement = drift
            bool isActivelyMovingHorizontally = movementMagnitude > HORIZONTAL_MOVEMENT_THRESHOLD;
            
            // If player is not actively moving, apply drift correction
            if (!_isMovementInputActive && !isActivelyMovingHorizontally)
            {
                // If there's any horizontal drift
                if (movementMagnitude > DRIFT_THRESHOLD)
                {
                    _driftCorrectionFrames++;
                    
                    // Apply correction if drift persists for a few frames
                    if (_driftCorrectionFrames > MIN_DRIFT_FRAMES_BEFORE_CORRECTION)
                    {
                        // Use teleport with Lerp for gradual correction
                        // Note: Teleport is only called when drift persists, not every frame
                        // This provides the most reliable correction given Unturned's physics system
                        Vector3 correctedPos = Vector3.Lerp(currentPos, _targetPosition, DRIFT_CORRECTION_STRENGTH);
                        correctedPos.y = currentPos.y; // Preserve vertical position
                        
                        Player.Teleport(correctedPos);
                    }
                }
                else
                {
                    // No significant drift, reset counter and update target
                    _driftCorrectionFrames = 0;
                    _targetPosition = currentPos;
                }
            }
            else
            {
                // Player is actively moving, update target position
                _targetPosition = currentPos;
                _driftCorrectionFrames = 0;
            }
            
            _lastPosition = currentPos;
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