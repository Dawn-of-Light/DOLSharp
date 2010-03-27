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

			var backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

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

            InventoryItem useItem = player.UseItem;
            if (useItem == null || !ArtifactMgr.IsArtifactScroll(useItem))
                return;

			GameInventoryItem combinedScroll = GameInventoryItem.CreateFromTemplate("artifact_scroll");
			if (combinedScroll == null)
				return;

			combinedScroll.AddOwner(player);
			combinedScroll.Name = useItem.Name;
			combinedScroll.Item.Name = useItem.Name;

			var backpack = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            ArrayList removeItems = new ArrayList();
			removeItems.Add(useItem);
			bool combinesToBook = false;
            foreach (InventoryItem item in backpack)
            {
                if (item == null)
                    continue;

                if (ArtifactMgr.CanCombine(combinedScroll.Item, item))
                {
                    combinedScroll = ArtifactMgr.CombineScrolls(combinedScroll.Item, item, ref combinesToBook);
                    removeItems.Add(item);
					if (combinesToBook)
						break;
                }
            }

            player.Out.SendSpellEffectAnimation(player, player, 1, 0, false, 1);

			Artifact artifact = ArtifactMgr.GetArtifact(combinedScroll.Item);

			if (artifact == null)
				log.Warn(String.Format("Missing artifact for item '{0}'", combinedScroll.Name));
			else
			{
				String receiveMessage = (combinesToBook)
					? artifact.MessageReceiveBook
					: artifact.MessageReceiveScrolls;
				player.Out.SendMessage(String.Format(receiveMessage, combinedScroll.Name, useItem.Name),
					eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}

			if (player.ReceiveItem(player, combinedScroll))
				foreach (InventoryItem item in removeItems)
					player.Inventory.RemoveItem(item);
        }
    }
}
