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
using DOL.Language;

namespace DOL.GS
{
	[NPCGuildScript("Smith")]
	public class Blacksmith : GameNPC
	{
		private const string REPAIR_ITEM_WEAK = "repair item";

		#region Examine/Interact Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.YouTarget", GetName(0, false)));
			list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
			list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.GiveObject", GetPronoun(0, true)));
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

			TurnTo(player, 1000);

			SayTo(player, eChatLoc.CL_PopupWindow, LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.Say"));
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
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
				                                                  "Scripts.Blacksmith.StackedObjets", GetName(0, false)),
				                       eChatType.CT_System, eChatLoc.CL_SystemWindow);

				return false;
			}

			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
				case (int)eObjectType.Magical:
				case (int)eObjectType.Instrument:
				case (int)eObjectType.Poison:
					SayTo(player, LanguageMgr.GetTranslation(player.Client,
					                                         "Scripts.Blacksmith.CantRepairThat"));

					return false;
			}

			if (item.Condition < item.MaxCondition)
			{
				if (item.Durability <= 0)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
					                                                  "Scripts.Blacksmith.ObjectCantRepaired"), eChatType.CT_System,
					                       eChatLoc.CL_SystemWindow);

					return false;
				}
				else
				{
					player.TempProperties.setProperty(REPAIR_ITEM_WEAK, new WeakRef(item));
					player.Client.Out.SendCustomDialog(LanguageMgr.GetTranslation(player.Client,
					                                                              "Scripts.Blacksmith.RepairCostAccept",
					                                                              Money.GetString(item.RepairCost), item.Name),
					                                   new CustomDialogResponse(BlacksmithDialogResponse));
				}
			}
			else
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
				                                                  "Scripts.Blacksmith.NoNeedRepair"), eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}

			return false;

		}

		protected void BlacksmithDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getProperty<object>(
					REPAIR_ITEM_WEAK,
					new WeakRef(null)
				);
			player.TempProperties.removeProperty(REPAIR_ITEM_WEAK);
			InventoryItem item = (InventoryItem)itemWeak.Target;

			if (response != 0x01)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
				                                                  "Scripts.Blacksmith.AbortRepair", item.Name), eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);

				return;
			}


			if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
			    || item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
				                                                  "Scripts.Blacksmith.InvalidItem"), eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);

				return;
			}

			int ToRecoverCond = item.MaxCondition - item.Condition;

			if (!player.RemoveMoney(item.RepairCost))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
				                                                  "Scripts.Blacksmith.NotEnoughMoney"), eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);

				return;
			}

			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.YouPay",
			                                                  GetName(0, false), Money.GetString(item.RepairCost)), eChatType.CT_System,
			                       eChatLoc.CL_SystemWindow);

			// Items with IsNotLosingDur are not....losing DUR.
			if (ToRecoverCond + 1 >= item.Durability)
			{
				item.Condition = item.Condition + item.Durability;
				item.Durability = 0;
				SayTo(player, LanguageMgr.GetTranslation(player.Client,
				                                         "Scripts.Blacksmith.ObjectRatherOld", item.Name));
			}
			else
			{
				item.Condition = item.MaxCondition;
				if (!item.IsNotLosingDur) item.Durability -= (ToRecoverCond + 1);
			}


			player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
			                                                  "Scripts.Blacksmith.ItsDone", item.Name), eChatType.CT_System,
			                       eChatLoc.CL_SystemWindow);

			return;
		}

		#endregion
	}
}
