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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[NPCGuildScript("Recharger")]
	public class Recharger : GameNPC
	{
		private const string RECHARGE_ITEM_WEAK = "recharged item";

		#region Examine/Interact Message

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and can recharge your equipment.");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player.X, player.Y);
			SayTo(player, eChatLoc.CL_ChatWindow, "I can recharge weapons or armor for you, Just hand me the item you want recharged and I'll see what I can do, for a small fee.");
			return true;
		}

		#endregion Examine/Interact Message

		#region Receive item

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer player = source as GamePlayer;
			if (player == null || item == null)
				return false;

			if (item.Count != 1)
			{
				player.Out.SendMessage(GetName(0, false) + " can't recharge stacked objects.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if((item.SpellID == 0 && item.SpellID1 == 0) ||
				(item.Object_Type == (int)eObjectType.Poison) ||
				(item.Object_Type == (int)eObjectType.Magical && (item.Item_Type == 40 || item.Item_Type == 41)))
			{
				SayTo(player, "I can't recharge that.");
				return false;
			}
			if(item.Charges == item.MaxCharges && item.Charges1 == item.MaxCharges1)
			{
				SayTo(player, "Item is fully charged!");
				return false;
			}

			long NeededMoney=0;
			if (item.Charges < item.MaxCharges)
			{
				player.TempProperties.setProperty(RECHARGE_ITEM_WEAK, new WeakRef(item));
				NeededMoney += (item.MaxCharges - item.Charges)*Money.GetMoney(0,0,10,0,0);
			}
			if (item.Charges1 < item.MaxCharges1)
			{
				player.TempProperties.setProperty(RECHARGE_ITEM_WEAK, new WeakRef(item));
				NeededMoney += (item.MaxCharges1 - item.Charges1)*Money.GetMoney(0,0,10,0,0);
			}
			if(NeededMoney > 0)
			{
				player.Client.Out.SendCustomDialog("It will cost " + Money.GetString(NeededMoney) + " to recharge that. Do you accept?", new CustomDialogResponse(RechargerDialogResponse));
				return true;
			}
			return false;
		}

		protected void RechargerDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getObjectProperty(
				RECHARGE_ITEM_WEAK,
				new WeakRef(null)
				);
			player.TempProperties.removeProperty(RECHARGE_ITEM_WEAK);

			InventoryItem item = (InventoryItem) itemWeak.Target;

			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == /*null*/0 || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
			{
				player.Out.SendMessage("You decline to have your " + item.Name + " recharged.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			long cost = 0;
			if (item.Charges < item.MaxCharges)
			{
				cost += (item.MaxCharges - item.Charges)*Money.GetMoney(0,0,10,0,0);
			}

			if (item.Charges1 < item.MaxCharges1)
			{
				cost += (item.MaxCharges1 - item.Charges1)*Money.GetMoney(0,0,10,0,0);
			}

			if(!player.RemoveMoney(cost))
			{
				player.Out.SendMessage("You don't have enough money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			player.Out.SendMessage("You give to " + GetName(0, false) + " " + Money.GetString((long) cost) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			item.Charges = item.MaxCharges;
			item.Charges1 = item.MaxCharges1;

			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SayTo(player, "There, it is now fully charged!");
			return;
		}

		#endregion Receive item
	}
}