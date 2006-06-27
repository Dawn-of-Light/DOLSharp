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

namespace DOL.GS.Scripts
{
	[NPCGuildScript("Enchanter")]
	public class Enchanter : GameMob
	{
		private const string ENCHANT_ITEM_WEAK = "enchanting item";
		private const string TOWARDSTR = " towards you.";
		private int[] BONUS_TABLE = new int[] {5, 5, 10, 15, 20, 25, 30, 30};

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList(2);
			string AggroString = GetAggroLevelString(player, false);

			if (AggroString.EndsWith(TOWARDSTR))
			{
				AggroString = AggroString.Remove(AggroString.Length - TOWARDSTR.Length, TOWARDSTR.Length);
			}

			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + AggroString + " and is an enchanter.");
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (base.Interact(player))
			{
				TurnTo(player, 25000);
				string Material;
				if (player.Realm == (byte) eRealm.Hibernia)
					Material = "quartz";
				else
					Material = "steel";

				SayTo(player, eChatLoc.CL_ChatWindow, "I can enchant weapons or armor that are of " + Material + " or better material. Just hand me the weapon you would like enchanted and I will work my magic upon it, for a fee.");
				return true;
			}
			return false;
		}

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			GamePlayer t = source as GamePlayer;
			if (t == null || item == null)
				return false;

			if (item.Level >= 10 && item.CrafterName != "")
			{
				if (item.Object_Type != (int) eObjectType.Magical && item.Object_Type != (int) eObjectType.Bolt && item.Object_Type != (int) eObjectType.Poison)
				{
					if (item.Bonus == 0)
					{
						t.TempProperties.setProperty(ENCHANT_ITEM_WEAK, new WeakRef(item));
						t.Client.Out.SendCustomDialog("It will cost " + Money.GetString(CalculEnchantPrice(item)) + "\x000ato enchant that. Do you accept?", new CustomDialogResponse(EnchanterDialogResponse));
					}
					else
						SayTo(t, eChatLoc.CL_SystemWindow, "This item is already enchanted!");
				}
				else
					SayTo(t, eChatLoc.CL_SystemWindow, "This item can't be enchanted!");
			}
			else
				SayTo(t, eChatLoc.CL_SystemWindow, "I can't enchant that material.");

			return false;
		}

		protected void EnchanterDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getObjectProperty(
					ENCHANT_ITEM_WEAK,
					new WeakRef(null)
					);
			player.TempProperties.removeProperty(ENCHANT_ITEM_WEAK);


			if (response != 0x01 || !WorldMgr.CheckDistance(this, player,WorldMgr.INTERACT_DISTANCE))
				return;

			InventoryItem item = (InventoryItem) itemWeak.Target;
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
				player.Out.SendMessage("Invalid item.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			long Fee = CalculEnchantPrice(item);

			if (player.GetCurrentMoney() < Fee)
			{
				SayTo(player, eChatLoc.CL_SystemWindow, "I need " + Money.GetString(Fee) + " to enchant that.");
				return;
			}
			if (item.Level < 50)
				item.Bonus = BONUS_TABLE[(item.Level/5) - 2];
			else
				item.Bonus = 35;

			item.Name = "bright " + item.Name;
			player.Out.SendInventoryItemsUpdate(new InventoryItem[] {item});
			player.Out.SendMessage("You give " + GetName(0, false) + " " + Money.GetString(Fee) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			player.RemoveMoney(Fee, null);
			SayTo(player, eChatLoc.CL_SystemWindow, "There, it is now " + item.GetName(1, false) + "!");
			return;
		}

		public long CalculEnchantPrice(InventoryItem item)
		{
			return (Money.GetMoney(0, 0, item.Gold, item.Silver, item.Copper)/5);
		}
	}
}