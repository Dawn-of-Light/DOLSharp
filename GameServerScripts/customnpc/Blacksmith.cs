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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[NPCGuildScript("Smith")]
	public class Blacksmith : GameMob
	{
		private const string REPEAR_ITEM_WEAK = "repear item";

		#region Examine/Interact Message

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and can repear your equipment.");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player.Position);
			SayTo(player, eChatLoc.CL_ChatWindow, "I can repair weapons or armor for you, Just hand me the item you want repaired and I'll see what I can do, for a small fee.");
			return true;
		}

		#endregion Examine/Interact Message

		#region Receive item

        public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null || item == null)
				return false;
			
			EquipableItem itemToRepair = item as EquipableItem;
			if (itemToRepair == null)
			{
				player.Out.SendMessage(GetName(0, false) + " can't repear this objet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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

		protected void BlacksmithDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getObjectProperty(
					REPEAR_ITEM_WEAK,
					new WeakRef(null)
					);
			player.TempProperties.removeProperty(REPEAR_ITEM_WEAK);

			if (response != 0x01)
				return;

			EquipableItem item = itemWeak.Target as EquipableItem;

			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.Owner == null || item.Owner != player)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			int ToRecoverCond = (int)(100 - item.Condition);
			long cost = ToRecoverCond*item.Value/100;

			if (!player.RemoveMoney(cost))
			{
				player.Out.SendMessage("You don't have enough money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			player.Out.SendMessage("You give to " + GetName(0, false) + " " + Money.GetString((long) cost) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			if (ToRecoverCond >= item.Durability)
			{
				item.Condition += item.Durability;
				item.Durability = 0;
				SayTo(player, "Uhh, that " + item.Name + " was already rather old, I won't be able to repair it once again, so be careful!");
			}
			else
			{
				item.Condition = 100;
				item.Durability -= (byte)(ToRecoverCond + 1);
				SayTo(player, "Well, it's finished. Your " + item.Name + " is practically new. Come back if you need my service once again!");
			}

			player.Out.SendInventorySlotsUpdate(new int[] {item.SlotPosition});
			SayTo(player, "It's ok. Now you can use your " + item.Name + " in fight!");
			return;
		}

		#endregion Receive item
	}
}