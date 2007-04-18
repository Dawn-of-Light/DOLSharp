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

namespace DOL.GS.Scripts
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
			/*
			 * You examine Elvar Ironhand. He is friendly and is a smith.
			 * [Give him an object to be repaired]
			 */
			IList list = new ArrayList();
			//list.Add("You target [" + GetName(0, false) + "]");
            /*
            list.Add("You examine " + GetName(0, false) + "  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a smith.");
			list.Add("[Give him an object to be repaired]");
            */
            list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.YouExamine", GetName(0, false), GetAggroLevelString(player, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.GiveObject")); 
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

            SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client, "Scripts.Blacksmith.Say"));
			//SayTo(player, eChatLoc.CL_ChatWindow, "I can repair weapons or armor for you, Just hand me the item you want repaired and I'll see what I can do, for a small fee of course.");
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
				player.Out.SendMessage(GetName(0, false) + " can't repair stacked objets.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			switch (item.Object_Type)
			{
				case (int)eObjectType.GenericItem:
				case (int)eObjectType.Magical:
				case (int)eObjectType.Instrument:
				case (int)eObjectType.Poison:
					SayTo(player, "I can't repair that.");
					return false;
			}
			if (item.Condition < item.MaxCondition)
			{
				if (item.Durability <= 0)
				{
					player.Out.SendMessage("This object can't be repaired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				else
				{
					player.TempProperties.setProperty(REPAIR_ITEM_WEAK, new WeakRef(item));
					long NeededMoney = ((item.MaxCondition - item.Condition) * item.Value) / item.MaxCondition;
					player.Out.SendMessage("It will cost " + Money.GetString(NeededMoney) + " to repair the " + item.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					player.Client.Out.SendCustomDialog("Do you accept to repair the " + item.Name, new CustomDialogResponse(BlacksmithDialogResponse));
				}
			}
			else
			{
				player.Out.SendMessage("This object doesn't need to be repaired.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return false;
		}

		protected void BlacksmithDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getObjectProperty(
					REPAIR_ITEM_WEAK,
					new WeakRef(null)
					);
			player.TempProperties.removeProperty(REPAIR_ITEM_WEAK);

			if (response != 0x01)
				return;

			InventoryItem item = (InventoryItem)itemWeak.Target;

			if (item == null || item.SlotPosition == (int)eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			int ToRecoverCond = item.MaxCondition - item.Condition;
			long cost = ToRecoverCond * item.Value / item.MaxCondition;

			if (!player.RemoveMoney(cost))
			{
				player.Out.SendMessage("You don't have enough money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			player.Out.SendMessage("You pay " + GetName(0, false) + " " + Money.GetString((long)cost) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			if (ToRecoverCond + 1 >= item.Durability)
			{
				item.Condition = item.Condition + item.Durability;
				item.Durability = 0;
				SayTo(player, "Uhh, that " + item.Name + " was already rather old, I won't be able to repair it once again, so be careful!");
			}
			else
			{
				item.Condition = item.MaxCondition;
				item.Durability -= (ToRecoverCond + 1);
				SayTo(player, "Well, it's finished. Your " + item.Name + " is practically new. Come back if you need my service once again!");
			}
			/*
			// Add some random Quality +1/-1 stuff to make smithing more interesting
			// Chance of quality -1 must be higher than +1 to reduce chance ob smithing abuse.
			if (item.Quality > 0 && Util.Chance(10)) // 10% chance to reduce item quality by one
			{
				item.Quality--;
			}
			else
			{
				int proChance = (item.MaxQuality - item.Quality) / 2;
				if (Util.Chance(proChance))
				{
					item.Quality++;
				}
			}*/

			player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
			SayTo(player, "It's done. Your " + item.Name + " is ready for combat!");
			return;
		}

		#endregion Receive item
	}
}
