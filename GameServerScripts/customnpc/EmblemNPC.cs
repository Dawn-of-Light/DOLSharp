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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// This class holds all information that
	/// EVERY emblemer npc in the game world needs!
	/// </summary>
	[NHibernate.Mapping.Attributes.Subclass(NameType=typeof(EmblemNPC), ExtendsType=typeof(GameMob))] 
	public class EmblemNPC : GameMob
	{
		public const long EMBLEM_COST = 50000;
		private const string EMBLEMIZE_ITEM_WEAK = "emblemise item";

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 10000);
			SayTo(player, eChatLoc.CL_ChatWindow, "For a 5 gold, I can put the emblem of your guild on the item. Just hand me the item.");

			return true;
		}

		/// <summary>
		/// Called when the object is about to get an item from someone
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
        public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null || item == null)
				return false;
			
			if(!(item is Cloak) && !(item is Shield))
			{
				player.Out.SendMessage("I can not put an emblem on this item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (((VisibleEquipment)item).Color != 0) //TODO only check if it is a emblem
			{
				player.Out.SendMessage("This item already has an emblem on it.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Guild == null)
			{
				player.Out.SendMessage("You have no guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.Guild.Emblem == 0)
			{
				player.Out.SendMessage("Your guild has no emblem.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.Level < 20 && (player.CraftingPrimarySkill == eCraftingSkill.NoCrafting || player.GetCraftingSkillValue(player.CraftingPrimarySkill) < 400)) //if level of player < 20 so can not put emblem
			{
				player.Out.SendMessage("You have to be at least level 20 or have 400 in a tradeskill to be able to wear an emblem.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (!player.Guild.CheckGuildPermission(player, eGuildPerm.Emblem))
			{
				player.Out.SendMessage("You do not have enough privileges for that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			player.TempProperties.setProperty(EMBLEMIZE_ITEM_WEAK, new WeakRef(item));
			player.Out.SendCustomDialog("Do you agree to put an emblem\x000Aon this object?", new CustomDialogResponse(EmblemerDialogResponse));

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// The callback hook used when a player answear to the emblemer dialog
		/// </summary>
		protected void EmblemerDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(EMBLEMIZE_ITEM_WEAK, new WeakRef(null));
			player.TempProperties.removeProperty(EMBLEMIZE_ITEM_WEAK);

			if (!Position.CheckDistance(player.Position, WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to speak with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
				return; //declined

			GenericItem item = (GenericItem) itemWeak.Target;

			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.Owner == null)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!player.RemoveMoney(EMBLEM_COST))
			{
				player.Out.SendMessage("You don't have enough money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			((VisibleEquipment)item).Color = player.Guild.Emblem;
			player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});

			if (item.SlotPosition < (int) eInventorySlot.FirstBackpack)
			{
				player.UpdateEquipementAppearance();
			}
			SayTo(player, eChatLoc.CL_ChatWindow, "I have put an emblem on your item.");
			return;
		}
	}
}