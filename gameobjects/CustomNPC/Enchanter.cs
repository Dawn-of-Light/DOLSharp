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
using DOL.Language;

namespace DOL.GS
{
	[NPCGuildScript("Enchanter")]
	public class Enchanter : GameNPC
	{
		private const string ENCHANT_ITEM_WEAK = "enchanting item";
		private int[] BONUS_TABLE = new int[] {5, 5, 10, 15, 20, 25, 30, 30};

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
            IList list = new ArrayList();
            list.Add(LanguageMgr.GetTranslation(player.Client, "Enchanter.GetExamineMessages.Text1", GetName(0, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client, "Enchanter.GetExamineMessages.Text2", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
            return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (base.Interact(player))
			{
				TurnTo(player, 25000);
				string Material;
				if (player.Realm == eRealm.Hibernia)
                    Material = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Enchanter.Interact.Text1");
				else
                    Material = LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Enchanter.Interact.Text2");

                SayTo(player, eChatLoc.CL_ChatWindow, LanguageMgr.GetTranslation(player.Client, "Enchanter.Interact.Text3", Material));
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
                        t.Client.Out.SendCustomDialog(LanguageMgr.GetTranslation(t.Client, "Enchanter.ReceiveItem.Text1", Money.GetString(CalculEnchantPrice(item))), new CustomDialogResponse(EnchanterDialogResponse));
                    }
					else
                        SayTo(t, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(t.Client, "Enchanter.ReceiveItem.Text2"));
                }
				else
                    SayTo(t, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(t.Client, "Enchanter.ReceiveItem.Text3"));
            }
			else
                SayTo(t, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(t.Client, "Enchanter.ReceiveItem.Text4"));

			return false;
		}

		protected void EnchanterDialogResponse(GamePlayer player, byte response)
		{
			WeakReference itemWeak =
				(WeakReference) player.TempProperties.getProperty<object>(
					ENCHANT_ITEM_WEAK,
					new WeakRef(null)
					);
			player.TempProperties.removeProperty(ENCHANT_ITEM_WEAK);


			if (response != 0x01 || !this.IsWithinRadius(player, WorldMgr.INTERACT_DISTANCE))
				return;

			InventoryItem item = (InventoryItem) itemWeak.Target;
			if (item == null || item.SlotPosition == (int) eInventorySlot.Ground
				|| item.OwnerID == null || item.OwnerID != player.InternalID)
			{
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Enchanter.EnchanterDialogResponse.Text1"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
			}

			long Fee = CalculEnchantPrice(item);

			if (player.GetCurrentMoney() < Fee)
			{
                SayTo(player, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(player.Client, "Enchanter.EnchanterDialogResponse.Text2", Money.GetString(Fee)));
                return;
			}
			if (item.Level < 50)
				item.Bonus = BONUS_TABLE[(item.Level/5) - 2];
			else
				item.Bonus = 35;

            item.Name = LanguageMgr.GetTranslation(player.Client, "Enchanter.EnchanterDialogResponse.Text3") + " " + item.Name;
            player.Out.SendInventoryItemsUpdate(new InventoryItem[] { item });
            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Enchanter.EnchanterDialogResponse.Text4", GetName(0, false), Money.GetString(Fee)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            player.RemoveMoney(Fee, null);
            SayTo(player, eChatLoc.CL_SystemWindow, LanguageMgr.GetTranslation(player.Client, "Enchanter.EnchanterDialogResponse.Text5", item.GetName(1, false)));
            return;
		}

		public long CalculEnchantPrice(InventoryItem item)
		{
			return (Money.GetMoney(0, 0, item.Gold, item.Silver, item.Copper)/5);
		}
	}
}