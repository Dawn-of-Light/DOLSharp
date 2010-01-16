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
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	[NPCGuildScript("Recharger")]
	public class Recharger : GameNPC
	{
		private const string RECHARGE_ITEM_WEAK = "recharged item";

		#region Examine/Interact Message

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.GetExamineMessages", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player.X, player.Y);
			SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.Interact"));
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
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.ReceiveItem.StackedObjects", GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if((item.SpellID == 0 && item.SpellID1 == 0) ||
				(item.Object_Type == (int)eObjectType.Poison) ||
				(item.Object_Type == (int)eObjectType.Magical && (item.Item_Type == 40 || item.Item_Type == 41)))
			{
				SayTo(player, LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.ReceiveItem.CantThat"));
				return false;
			}
			if(item.Charges == item.MaxCharges && item.Charges1 == item.MaxCharges1)
			{
				SayTo(player, LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.ReceiveItem.FullyCharged"));
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
				player.Client.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.ReceiveItem.Cost", Money.GetString(NeededMoney)), new CustomDialogResponse(RechargerDialogResponse));
				return true;
			}
			return false;
		}

		protected void RechargerDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getProperty<object>(
				RECHARGE_ITEM_WEAK,
				new WeakRef(null)
				);
			player.TempProperties.removeProperty(RECHARGE_ITEM_WEAK);

			InventoryItem item = (InventoryItem) itemWeak.Target;

			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.RechargerDialogResponse.InvalidItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (response != 0x01)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.RechargerDialogResponse.Decline", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.RechargerDialogResponse.NotMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.RechargerDialogResponse.GiveMoney", GetName(0, false), Money.GetString((long)cost)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			item.Charges = item.MaxCharges;
			item.Charges1 = item.MaxCharges1;

			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			SayTo(player, LanguageMgr.GetTranslation(player.Client, "Scripts.Recharger.RechargerDialogResponse.FullyCharged"));
			return;
		}

		#endregion Receive item
	}
}