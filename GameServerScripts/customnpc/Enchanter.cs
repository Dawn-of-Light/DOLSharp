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
//12/13/2004
//Written by Gavinius
//based on Nardin and Zjovaz previous script


using System;
using System.Collections;
using DOL.Database;
using DOL.GS.PacketHandler;
using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// This class holds all information that
	/// EVERY enchanter npc in the game world needs!
	/// </summary>
	[Subclass(NameType=typeof(Enchanter), ExtendsType=typeof(GameMob))] 
	public class Enchanter : GameMob
	{
		private const string ENCHANT_ITEM_WEAK = "enchanting item";
		private byte[] BONUS_TABLE = new byte[] {5, 5, 10, 15, 20, 25, 30, 30};

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is an enchanter.");
			list.Add("[Right click to display a shop window]");
			return list;
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			
			TurnTo(player, 10000);
			string Material;
			if (player.Realm == (byte) eRealm.Hibernia)
				Material = "quartz";
			else
				Material = "steel";

			SayTo(player, eChatLoc.CL_ChatWindow, "I can enchant weapons or armor that are of " + Material + " or better material. Just hand me the weapon you would like enchanted and I will work my magic upon it, for a fee.");
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

			if (item.Level >= 10 && item.CrafterName != "")
			{
				if (item is EquipableItem)
				{
					if (((EquipableItem)item).Bonus == 0)
					{
						player.TempProperties.setProperty(ENCHANT_ITEM_WEAK, new WeakRef(item));
						player.Client.Out.SendCustomDialog("It will cost " + Money.GetString(item.Value / 5) + "\x000ato enchant that. Do you accept?", new CustomDialogResponse(EnchanterDialogResponse));
					}
					else
						SayTo(player, eChatLoc.CL_SystemWindow, "This item is already enchanted!");
				}
				else
					SayTo(player, eChatLoc.CL_SystemWindow, "This item can't be enchanted!");
			}
			else
				SayTo(player, eChatLoc.CL_SystemWindow, "I can't enchant that material.");

			return base.ReceiveItem(source, item);
		}

		/// <summary>
		/// The callback hook used when a player answear to the enchant dialog
		/// </summary>
		protected void EnchanterDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(ENCHANT_ITEM_WEAK, new WeakRef(null));
			player.TempProperties.removeProperty(ENCHANT_ITEM_WEAK);

			if (!Position.CheckDistance(player.Position, WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to speak with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
				return;

			EquipableItem item = (EquipableItem) itemWeak.Target;
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.Owner == null || item.Owner  != player)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			long fee = item.Value / 5;
			if (!player.RemoveMoney(fee, "You give "+ GetName(0, false) + " {0}."))
			{
				SayTo(player, eChatLoc.CL_SystemWindow, "I need " + Money.GetString(fee) + " to enchant that.");
				return;
			}

			if (item.Level < 50)
			{
				item.Bonus = BONUS_TABLE[(item.Level/5) - 2];
			}
			else
			{
				item.Bonus = 35;
			}

			item.Name = "bright " + item.Name;
			player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});

			SayTo(player, eChatLoc.CL_SystemWindow, "There, it is now " + item.Name + "!");
			return;
		}
	}
}