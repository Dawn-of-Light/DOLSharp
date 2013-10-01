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
using System.Collections.Generic;

using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Language;

namespace DOL.GS
{
	// This class has to be completed and may be inherited for scripting purpose (like quests)
	public class KingNPC : GameNPC
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public KingNPC()
			: base()
		{
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			TurnTo(player, 5000);
			if (!player.Champion && player.Level == 50)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "KingNPC.WhisperReceive.AskForChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}

			if (player.Champion)
			{
				bool cllevel = false;

				while (player.ChampionLevel < player.ChampionMaxLevel && player.ChampionExperience >= player.ChampionExperienceForNextLevel)
				{
					player.ChampionLevelUp();
					cllevel = true;
				}

				if ( cllevel )
				{
					player.Out.SendMessage( "You reached champion level " + player.ChampionLevel + "!", eChatType.CT_System, eChatLoc.CL_PopupWindow );
				}

				if (player.ChampionLevel >= 5)
				{
					player.Out.SendMessage("I can [respecialize] your champion skills if you so desire.", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}

			}

			return true;
		}

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;

			GamePlayer player = source as GamePlayer;
			if (player == null) return false;

			if (str == "Champions" && player.Level == 50)
			{
				if (player.Champion == true)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "KingNPC.WhisperReceive.AlreadyChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
					return false;
				}

				player.RemoveChampionLevels();
				player.Champion = true;
				player.Out.SendUpdatePlayer();
				player.SaveIntoDatabase();
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "KingNPC.WhisperReceive.IsNowChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return true;
			}

			if (str.ToLower() == "respecialize" && player.Champion && player.ChampionLevel >= 5)
			{
				player.RespecChampionSkills();
				player.SaveIntoDatabase();
				player.Out.SendMessage("I have reset your champion skills!", eChatType.CT_Important, eChatLoc.CL_PopupWindow);
			}

			return true;
		}
	}

	// This class has to be completed and may be inherited for scripting purpose
	public class CLWeaponNPC : GameNPC
	{
		private static IList<ItemTemplate> m_champ_weapons = GameServer.Database.SelectObjects<ItemTemplate>("PackageID LIKE 'Champion_Weapons'");
		
		public CLWeaponNPC()
			: base()
		{
		}
		
		public override bool Interact(GamePlayer player)
		{
			if(!base.Interact(player))
				return false;
			
			//check if player if champ level 5 or higher...
			if(player.Champion && player.ChampionLevel >= 5)
			{
				//Offer Champion Weapons
				player.Out.SendMessage("Here are the weapons offered by our King for your achievements in the Realm :\n", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				
				string proposed = "";
				foreach(ItemTemplate chwep in m_champ_weapons)
					if(chwep.AllowedClasses.SplitCSV().Contains(player.CharacterClass.ID.ToString()))
						proposed += proposed.Length < 1 ? "["+chwep.Name+"]" : ", ["+chwep.Name+"]";
				
				player.Out.SendMessage(proposed, eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}
			else
			{
				//Not worthy enough
				player.Out.SendMessage("You need to reach Champion Level 5 before I can entrust you with King's Weapons !", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}
			return true;
		}
		
		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;

			GamePlayer player = source as GamePlayer;
			
			if (player == null)
				return false;

			if(text == null || text.Length < 1)
				return false;

			
			//check if he can reiceive weapon
			if(player.Champion && player.ChampionLevel >= 5)
				foreach(ItemTemplate chwep in m_champ_weapons)
					if(chwep.Name.ToLower().Equals(text.ToLower()) && chwep.AllowedClasses.SplitCSV().Contains(player.CharacterClass.ID.ToString()))
						//can use it, have correct level, and asked it... maybe we should give him !
						if(player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, GameInventoryItem.Create<ItemTemplate>(chwep)))
							player.Out.SendMessage("Here's the weapon you asked. Fight Well !", eChatType.CT_System, eChatLoc.CL_PopupWindow);
						else
							player.Out.SendMessage("Your inventory is full, I can't give you this weapon !", eChatType.CT_System, eChatLoc.CL_PopupWindow);

			//Now we turn the npc into the direction of the person
			TurnTo(player, 10000);
			return true;
		}

		/// <summary>
		/// For Recieving CL Respec Stone.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			GamePlayer player = source as GamePlayer;
			if (player != null)
			{
				switch (item.Id_nb)
				{
					case "respec_cl":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
                            InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Other, item.Template);
							player.RespecAmountChampionSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "CLWeaponNPC.ReceiveItem.RespecCL"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
				}
			}

			return base.ReceiveItem(source, item);
		}
	}
}
