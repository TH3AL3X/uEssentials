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

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Metadata;
using Essentials.Common;
using Essentials.Common.Util;
using Essentials.Core;
using Rocket.API;
using Rocket.Core;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Essentials.Api.Unturned {

    public sealed class UPlayer : ICommandSource {

        public UnturnedPlayer RocketPlayer { get; }
        public string DisplayName => CharacterName;
        public string Id => CSteamId.m_SteamID.ToString();
        public string CharacterName => RocketPlayer.CharacterName;
        public string SteamName => RocketPlayer.SteamName;
        public float Rotation => RocketPlayer.Rotation;
        public bool IsOnGround => UnturnedPlayer.movement.isGrounded;
        public byte Stamina => Life.stamina;
        public bool IsDead => Life.isDead;
        public bool IsPro => SteamPlayer.isPro;
        public bool IsInVehicle => CurrentVehicle != null;
        public uint Ping => (uint) (RocketPlayer.Player.channel.owner.ping*1000);
        public SteamChannel Channel => UnturnedPlayer.channel;
        public List<string> Permissions => R.Permissions.GetPermissions(RocketPlayer).Select(p => p.Name).ToList();
        public Player UnturnedPlayer => RocketPlayer.Player;
        public SteamPlayer SteamPlayer => Channel.owner;
        public Vector3 Position => RocketPlayer.Position;
        public PlayerInventory Inventory => RocketPlayer.Inventory;
        public InteractableVehicle CurrentVehicle => UnturnedPlayer.movement.getVehicle();
        public CSteamID CSteamId => SteamPlayer.playerID.steamID;
        public PlayerMovement Movement => UnturnedPlayer.movement;
        public PlayerLook Look => UnturnedPlayer.look;
        public PlayerClothing Clothing => UnturnedPlayer.clothing;
        public PlayerLife Life => UnturnedPlayer.life;
        public PlayerEquipment Equipment => UnturnedPlayer.equipment;
        public EPlayerStance Stance => UnturnedPlayer.stance.stance;
        public bool IsOnline => RocketPlayer != null && UnturnedPlayer != null;
        public MetadataStore<object> Metadata { get; private set; }
        public PlayerInput Input => UnturnedPlayer.input;

        private readonly FieldInfo _experienceField = ReflectUtil.GetField<PlayerSkills>("_experience");

        bool ICommandSource.IsConsole => false;

        internal UPlayer(UnturnedPlayer player) {
            Metadata = new MetadataStore<object>();
            RocketPlayer = player;
        }

        public byte Health {
            get { return RocketPlayer.Health; }
            set {
                var playerHealth = RocketPlayer.Health;

                if (playerHealth > value) {
                    RocketPlayer.Damage(
                        (byte) (playerHealth - value),
                        Vector3.zero,
                        EDeathCause.BLEEDING,
                        ELimb.SPINE,
                        CSteamId
                    );
                } else {
                    RocketPlayer.Heal((byte) (playerHealth - value));
                }

                RocketPlayer.Bleeding = false;
            }
        }

        public uint Experience {
            get { return RocketPlayer.Experience; }
            set {
                UnturnedPlayer.skills.ServerSetExperience(value);
            }
        }

        public byte Hunger {
            get { return RocketPlayer.Hunger; }
            set { RocketPlayer.Hunger = (byte) (100 - value); }
        }

        public byte Thirst {
            get { return RocketPlayer.Thirst; }
            set { RocketPlayer.Thirst = (byte) (100 - value); }
        }

        public byte Infection {
            get { return RocketPlayer.Infection; }
            set { RocketPlayer.Infection = (byte) (100 - value); }
        }

        public bool IsBleeding {
            get { return Life.isBleeding; }
            set {
                // Life.Bleeding = value; No setter...
                RocketPlayer.Bleeding = value;
            }
        }

        public bool IsBroken {
            get { return Life.isBleeding; }
            set {
                // Life.Broken = value; No setter...
                RocketPlayer.Broken = value;
            }
        }

        public bool IsAdmin {
            get { return RocketPlayer.IsAdmin; }
            set { RocketPlayer.Admin(value); }
        }

        public void Teleport(Vector3 pos) {
            Teleport(pos, RocketPlayer.Rotation);
        }

        public void Teleport(Vector3 pos, float rotation) {
            RocketPlayer.Teleport(pos, rotation);
        }

        public bool HasPermission(string permission) {
            return IsAdmin || RocketPlayer.HasPermission(permission);
        }

        public void SendMessage(object message, Color color) {
            UnturnedChat.Say(RocketPlayer, message?.ToString() ?? "null", color);
        }

        public void SendMessage(object message) {
            UnturnedChat.Say(RocketPlayer, message?.ToString() ?? "null");
        }

        public bool GiveItem(Item item, ushort amount, bool dropIfInventoryIsFull = false) {
            var added = false;

            for (var i = 0; i < amount; i++) {
                var clone = new Item(item.id, item.amount, item.durability, item.metadata);

                added = UnturnedPlayer.inventory.tryAddItem(clone, true);

                if (!added && dropIfInventoryIsFull) {
                    ItemManager.dropItem(clone, Position, true, true, true);
                }
            }

            return added;
        }

        public bool GiveItem(Item item, bool dropIfInventoryIsFull = false) {
            return GiveItem(item, item.amount, dropIfInventoryIsFull);
        }

        public void DispatchCommand(string command) {
            if (string.IsNullOrEmpty(command)) return;

            if (command.StartsWith("/"))
                command = command.Substring(1);

            R.Commands.Execute(RocketPlayer, command);
        }

        public void Kill() {
            UnturnedPlayer.life.askDamage(
                100,
                Position.normalized,
                EDeathCause.KILL,
                ELimb.SKULL,
                CSteamID.Nil,
                out _
                );
        }

        public void Suicide() {
            RocketPlayer.Suicide();
        }

        public void Kick(string reason) {
            Provider.kick(CSteamId, reason);
        }

        public void Kick() {
            Kick("Undefined");
        }

        public void SetSkillLevel(USkill uSkill, byte value) {
            UnturnedPlayer.skills.ServerSetSkillLevel(uSkill.SpecialityIndex, uSkill.SkillIndex, value);
        }

        public byte GetSkillLevel(USkill uSkill) {
            return GetSkill(uSkill).level;
        }

        public Skill GetSkill(USkill uSkill) {
            var skills = UnturnedPlayer.skills;
            return skills.skills[uSkill.SpecialityIndex][uSkill.SkillIndex];
        }

        public Vector3? GetEyePosition(float distance, int masks) {
            Physics.Raycast(Look.aim.position, Look.aim.forward, out var raycastHit, distance, masks);

            if (raycastHit.transform == null)
                return null;

            return raycastHit.point;
        }

        public Vector3? GetEyePosition(float distance) {
            return GetEyePosition(distance, RayMasks.BLOCK_COLLISION & ~(1 << 0x15));
        }

        public bool HasComponent<T>() where T : Component {
            return GetComponent<T>() != null;
        }

        public bool HasComponent(Type componentType) {
            return GetComponent(componentType) != null;
        }

        public void RemoveComponent<T>() where T : Component {
            UnityEngine.Object.Destroy(GetComponent<T>());
        }

        public void RemoveComponent(Type componentType) {
            UnityEngine.Object.Destroy(GetComponent(componentType));
        }

        public T AddComponent<T>() where T : Component {
            return UnturnedPlayer.gameObject.AddComponent<T>();
        }

        public Component AddComponent(Type componentType) {
            return UnturnedPlayer.gameObject.AddComponent(componentType);
        }

        public T GetComponent<T>() where T : Component {
            return UnturnedPlayer.gameObject.GetComponent<T>();
        }

        public Component GetComponent(Type componentType) {
            return UnturnedPlayer.gameObject.GetComponent(componentType);
        }

        public static UPlayer From(UnturnedPlayer player) {
            return player == null
                ? null
                : From(player.CSteamID.m_SteamID);
        }

        public static UPlayer From(Player player) {
            return player == null
                ? null
                : From(player.channel.owner.playerID.steamID.m_SteamID);
        }

        public static UPlayer From(string name, bool ignoreCase = true) {
            /*
                - equals > startwith > contains
                - characterName > steamName
            */

            var players = EssCore.Instance.ConnectedPlayers;

            var cmpFlags = ignoreCase ? 0 : StringComparison.OrdinalIgnoreCase;
            var bestMatch = new LinkedList<UPlayer>();
            var score = 0;
            var reverseCount = players.Count;

            foreach (var entry in players) {
                var current = entry.Value;
                var steamName = entry.Value.SteamName;
                var charName = entry.Value.CharacterName;

                if (string.Compare(name, charName, cmpFlags) == 0 ||
                    string.Compare(name, steamName, cmpFlags) == 0) {
                    return entry.Value;
                }

                if (charName.StartsWith(name, cmpFlags) ||
                    steamName.StartsWith(name, cmpFlags)) {
                    var curScore = 2*reverseCount;
                    if (curScore > score) {
                        bestMatch.AddFirst(current);
                        score = curScore;
                    } else {
                        bestMatch.AddLast(current);
                    }
                }

                if ((ignoreCase && charName.ContainsIgnoreCase(name)) || charName.Contains(name) ||
                    (ignoreCase && steamName.ContainsIgnoreCase(name)) || steamName.Contains(name)) {
                    var curScore = 1*reverseCount;
                    if (curScore > score) {
                        bestMatch.AddFirst(current);
                        score = curScore;
                    } else {
                        bestMatch.AddLast(current);
                    }
                }

                reverseCount--;
            }

            return bestMatch.Count == 0 ? null : bestMatch.First.Value;
        }

        public static UPlayer From(ulong rawCSteamId) {
            return EssCore.Instance.ConnectedPlayers.TryGetValue(rawCSteamId, out var ret)
                   ? ret : null;
        }

        public static UPlayer From(CSteamID csteamId) {
            return csteamId == CSteamID.Nil
                ? null
                : From(csteamId.m_SteamID);
        }

        public static UPlayer From(SteamPlayer player) {
            return player == null
                ? null
                : From(player.playerID.steamID);
        }

        /// <summary>
        /// Tries to find a player, if found then <paramref name="callback"/> will be called
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        public static bool TryGet(CSteamID id, Action<UPlayer> callback) {
            var player = From(id);

            if (player != null) {
                callback(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found then <paramref name="callback"/> will be called
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public static bool TryGet(string name, Action<UPlayer> callback) {
            var player = From(name);

            if (player != null) {
                callback(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found then <paramref name="callback"/> will be called
        /// </summary>
        /// <param name="rocketPlayer"></param>
        /// <param name="callback"></param>
        public static bool TryGet(UnturnedPlayer rocketPlayer, Action<UPlayer> callback) {
            var player = From(rocketPlayer);

            if (player != null) {
                callback(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found then <paramref name="callback"/> will be called
        /// </summary>
        /// <param name="unturnedPlayer"></param>
        /// <param name="callback"></param>
        public static bool TryGet(Player unturnedPlayer, Action<UPlayer> callback) {
            var player = From(unturnedPlayer);

            if (player != null) {
                callback(player);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to find a player, if found then <paramref name="callback"/> will be called
        /// </summary>
        /// <param name="cmdArg"></param>
        /// <param name="callback"></param>
        [Obsolete("Use TryGet(string name, out UPlayer player) instead.")]
        public static bool TryGet(ICommandArgument cmdArg, Action<UPlayer> callback) {
            var player = From(cmdArg.ToString());

            if (player != null) {
                callback(player);
                return true;
            }

            return false;
        }

        public static bool TryGet(string name, out UPlayer player) {
            player = From(name);
            return player != null;
        }

        public override string ToString() {
            return DisplayName;
        }

        public static bool operator ==(UPlayer left, ICommandSource right) {
            if (Equals(left, right))
                return true;

            return left?.Id == right?.Id;
        }

        public static bool operator !=(UPlayer left, ICommandSource right) {
            return !(left == right);
        }

        public bool Equals(UPlayer other) {
            return Equals(this, other);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            /*
                TODO; Improve
            */
            return RocketPlayer.Equals(((UPlayer) obj).RocketPlayer);
        }

        public override int GetHashCode() {
            return RocketPlayer.GetHashCode();
        }

    }

}