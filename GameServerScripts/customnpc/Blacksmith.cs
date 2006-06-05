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
using System.Collections;
using DOL.GS.PacketHandler;
using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// This class holds all information that
	/// EVERY blacksmith npc in the game world needs!
	/// </summary>
	[Subclass(NameType=typeof(Blacksmith), ExtendsType=typeof(GameMob))] 
	public class Blacksmith : GameMob
	{
		private const string REPEAR_ITEM_WEAK = "repair item";

		#region Examine/Interact Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			/*
			 * You examine Elvar Ironhand. He is friendly and is a smith.
			 * [Give him an object to be repaired]
			 */
			IList list = new ArrayList();
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a smith.");
			list.Add("[Give him an object to be repaired]");
			return list;
		}

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
			SayTo(player, eChatLoc.CL_ChatWindow, "I can repair weapons or armor for you, Just hand me the item you want repaired and I'll see what I can do, for a small fee.");
			return true;
		}

		#endregion Examine/Interact Message

		#region Receive item

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
			
			EquipableItem itemToRepair = item as EquipableItem;
			if (itemToRepair == null)
			{
				player.Out.SendMessage(GetName(0, false) + " can't repair this objet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			
			if (itemToRepair.Condition >= 100)
			{
				player.Out.SendMessage("This object doesn't need to be repaired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			
			}
			
			if (itemToRepair.Durability <= 0)
			{
				player.Out.SendMessage("This object can't be repaired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			
			player.TempProperties.setProperty(REPEAR_ITEM_WEAK, new WeakRef(itemToRepair));
			long NeededMoney = (long)((100 - itemToRepair.Condition) * item.Value / 100);
			player.Client.Out.SendCustomDialog(GetName(0, true) + " asks " + Money.GetString(NeededMoney) + "\n to repair " + item.Name, new CustomDialogResponse(BlacksmithDialogResponse));
				
			return false;
		}

		/// <summary>
		/// The callback hook used when a player answear to the blacksmith dialog
		/// </summary>
		protected void BlacksmithDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak = (WeakReference) player.TempProperties.getObjectProperty(REPEAR_ITEM_WEAK, new WeakRef(null));
			player.TempProperties.removeProperty(REPEAR_ITEM_WEAK);

			if (!Position.CheckDistance(player.Position, WorldMgr.INTERACT_DISTANCE*WorldMgr.INTERACT_DISTANCE))
			{
				player.Out.SendMessage("You are too far away to speak with " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
				return;

			EquipableItem item = itemWeak.Target as EquipableItem;

			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.Owner == null || item.Owner != player)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			double toRecoverCond = (int)(100 - item.Condition);
			long cost = (long)(toRecoverCond * item.Value / 100);

			if (!player.RemoveMoney(cost, "You give " + GetName(0, false) + " {0}."))
			{
				player.Out.SendMessage("You don't have enough money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (toRecoverCond >= item.Durability)
			{
				item.Condition += item.Durability;
				item.Durability = 0;
				SayTo(player, "Uhh, that " + item.Name + " is rather old, I won't be able to repair it again, so be careful!");
			}
			else
			{
				item.Condition = 100;
				item.Durability -= (byte)(toRecoverCond + 1);
				SayTo(player, "Well, it's finished. Your " + item.Name + " is practically new. Come back if you need my service once again!");
			}

			player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
			SayTo(player, "It's done. Your " + item.Name + " is ready for combat!");
			return;
		}

		#endregion Receive item
	}
}