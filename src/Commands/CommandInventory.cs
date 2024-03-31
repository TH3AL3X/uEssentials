#region License

/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2024 Terror
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
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.I18n;
using SDG.Unturned;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "sinventory",
        Description = "Modify iventory",
        Aliases = new[] { "si" },
        Usage = "edit <player | *> <height>x<width> | return <player | *> | sinventory"
    )]
    public class CommandIventory : EssCommand
    {
        public readonly byte[] EMPTY_BYTE_ARRAY = new byte[0];

        public override CommandResult OnExecute(ICommandSource src, ICommandArgs args)
        {
            if (args.IsEmpty && src.IsConsole)
            {
                return CommandResult.ShowUsage();
            }

            if (args.Length != 2)
            {
                return CommandResult.ShowUsage();
            }

            string targetString = args[0].ToString();
            string[] size = args[1].ToString().ToLower().Split('x');

            if (size.Length != 2 || !int.TryParse(size[0], out int height) || !int.TryParse(size[1], out int width))
            {
                return CommandResult.ShowUsage();
            }

            if (!args[0].IsValidPlayerIdentifier)
            {
                // specific player
                return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
            }

            InteractableStorage storage = args[0].ToPlayer.Inventory.storage;
            if (storage == null)
            {
                UnturnedChat.Say(caller, $"{args[0].ToPlayer.DisplayName} doesn't have a storage.", Color.red);
                return;
            }

            var player = (UnturnedPlayer)caller;
            player.Player.inventory.capacity = size;

            storage.width = width;
            storage.height = height;
            UnturnedChat.Say(caller, $"Storage size set to {height}x{width} for {targetPlayer.DisplayName}.", Color.green);


            if (args.IsEmpty)
            {
                // self
                ClearInventory(src.ToPlayer());
            }
            else if (args[0].Equals("*"))
            {
                // all
                if (!src.HasPermission($"{Permission}.all"))
                {
                    return CommandResult.NoPermission($"{Permission}.all");
                }

                UServer.Players.ForEach(ClearInventory);
                EssLang.Send(src, "INVENTORY_CLEARED_ALL");
            }
            else
            {
                if (!args[0].IsValidPlayerIdentifier)
                {
                    // specific player
                    return CommandResult.LangError("PLAYER_NOT_FOUND", args[0]);
                }

                if (!src.HasPermission($"{Permission}.other"))
                {
                    return CommandResult.NoPermission($"{Permission}.other");
                }

                ClearInventory(args[0].ToPlayer);
                EssLang.Send(src, "INVENTORY_CLEARED_PLAYER", args[0].ToPlayer.DisplayName);
            }

            return CommandResult.Success();
        }

        private void AdjustInventorySize(UnturnedPlayer player, int newSize)
        {
            var items = player.Inventory.storage.items.;
            player.Inventory.storage.items.clear();

            // Añadir espacios adicionales si el tamaño aumenta
            while (newSize > items.Count)
            {
                items.Add(new Item(0, true));
            }

            // Truncar el inventario si el tamaño disminuye
            items = items.Take(newSize).ToList();

            foreach (var item in items)
            {
                player.Inventory.tryAddItem(item);
            }
        }

        private void ClearInventory(UPlayer player)
        {
            var playerInv = player.Inventory;

            // "Remove "models" of items from player "body""
            for (byte index = 0; index < player.Inventory.getItemCount(0); index++)
            {
                var item = player.Inventory.getItem(0, index);
                if (item != null)
                    player.Inventory.removeItem(0, index);
            }

            player.Equipment.sendSlot(0);
            for (byte index = 0; index < player.Inventory.getItemCount(1); index++)
            {
                var item = player.Inventory.getItem(1, index);
                if (item != null)
                    player.Inventory.removeItem(1, index);
            }

            player.Equipment.sendSlot(1);

            // Remove items
            for (byte page = 0; page < PlayerInventory.PAGES; page++)
            {
                if (page == PlayerInventory.AREA)
                    continue;

                var count = playerInv.getItemCount(page);

                for (byte index = 0; index < count; index++)
                    playerInv.removeItem(page, 0);
            }

            // Remove clothes

            // Remove unequipped cloths
            void RemoveUnequipped()
            {
                for (byte i = 0; i < playerInv.getItemCount(2); i++)
                    playerInv.removeItem(2, 0);
            }

            // Unequip & remove from inventory
            player.UnturnedPlayer.clothing.askWearBackpack(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearGlasses(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearHat(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearPants(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearMask(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearShirt(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            player.UnturnedPlayer.clothing.askWearVest(0, 0, EMPTY_BYTE_ARRAY, true);
            RemoveUnequipped();

            EssLang.Send(player, "INVENTORY_CLEARED");
        }
    }
}