/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.Database;
using System.Collections;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// The spell that combines artifact scrolls.
    /// </summary>
    /// <author>Aredhel</author>
	[SpellHandlerAttribute("CombineScrolls")]
    class CombineScrolls : SpellHandler
    {
        public CombineScrolls(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine) { }

        /// <summary>
        /// Check whether it's actually possible to do the combine.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
			if (!base.CheckBeginCast(selectedTarget))
				return false;

            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return false;

			InventoryItem scroll = player.UseItem;
            if (scroll == null || !ArtifactMgr.IsArtifactScroll(scroll))
                return false;

            ICollection backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,
                eInventorySlot.LastBackpack);

			foreach (InventoryItem item in backpack)
				if (item != null && item != scroll)
					if (ArtifactMgr.CanCombine(scroll, item))
						return true;

            return false;
        }

        /// <summary>
        /// Do the combine.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return;

            InventoryItem scroll = player.UseItem;
            if (scroll == null || !ArtifactMgr.IsArtifactScroll(scroll))
                return;

            String scrollID = ArtifactMgr.GetScrollIDFromItem(scroll);
			if (scrollID == null)
				return;

            String artifactID = ArtifactMgr.GetArtifactIDFromScrollID(scrollID);
            if (artifactID == null)
                return;

            player.Inventory.RemoveItem(scroll);

            ICollection backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,
                eInventorySlot.LastBackpack);

            foreach (InventoryItem item in backpack)
            {
                if (item == null)
                    continue;

                if (ArtifactMgr.IsArtifactScroll(item) &&
                    item.Name.StartsWith(scrollID) &&
                    (item.Flags & scroll.Flags) == 0)
                {
                    scroll.Flags |= item.Flags;
                    player.Inventory.RemoveItem(item);

                    if ((scroll.Flags & (int)ArtifactMgr.Book.AllPages) == (int)ArtifactMgr.Book.AllPages)
                    {
                        scroll.Model = 500;
                        break;
                    }
                }
            }

            scroll.Name = ArtifactMgr.CreateScrollName(scroll, artifactID);
            player.Out.SendSpellEffectAnimation(player, player, 1, 0, false, 1);

            eInventorySlot slot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack,
                eInventorySlot.LastBackpack);

            if (slot != eInventorySlot.Invalid)
                player.Inventory.AddItem(slot, scroll);
            else
            {
                player.Out.SendMessage(String.Format("Your backpack is full. {0} is dropped on the ground.",
                    player.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                DropScroll(player, scroll);
            }
        }

        /// <summary>
        /// Drop a scroll to the ground.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="scroll"></param>
        private static void DropScroll(GamePlayer owner, InventoryItem scroll)
        {
            GameInventoryItem loot = new GameInventoryItem(scroll);
            loot.AddOwner(owner);
            loot.X = owner.X;
            loot.Y = owner.Y;
            loot.Z = owner.Z;
            loot.Heading = owner.Heading;
            loot.CurrentRegion = owner.CurrentRegion;
        }
    }
}
