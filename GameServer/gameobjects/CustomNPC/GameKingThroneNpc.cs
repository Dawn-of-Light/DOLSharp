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
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Language;

namespace DOL.GS
{
	// This class has to be completed and may be inherited for scripting purpose (like quests)
	public class KingNPC : GameNPC
	{
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
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "KingNPC.WhisperReceive.AskForChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
			}

			if (player.Champion)
			{
				bool cllevel = false;
				while (player.ChampionLevel < player.ChampionMaxLevel && player.ChampionExperience >= player.ChampionExperienceForNextLevel)
				{
					player.ChampionLevelUp();
					cllevel = true;
				}
				if (cllevel) //TODO: Out.Message (MLXP)
					player.Out.SendMessage("You reached champion level " + player.ChampionLevel + "! (ToDo: better text)", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				//player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "KingNPC.WhisperReceive.NewLevelMessage"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "KingNPC.WhisperReceive.AlreadyChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
					return false;
				}
				player.Champion = true;
				player.Out.SendUpdatePlayer();
				player.SaveIntoDatabase();
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "KingNPC.WhisperReceive.IsNowChampion"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return true;
			}
			return true;
		}
	}
	// This class has to be completed and may be inherited for scripting purpose
	public class CLWeaponNPC : GameNPC
	{
		public CLWeaponNPC()
			: base()
		{
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
			if (player == null) return false;

			switch (text)
			{
				//level respec for players
				case "respecialize":
					if (player.Champion && player.ChampionLevel >= 5)
					{
						player.RemoveSpellLine("Champion Abilities" + player.Name);
						SkillBase.UnRegisterSpellLine("Champion Abilities" + player.Name);
						player.ChampionSpells = "";
						player.ChampionSpecialtyPoints = player.ChampionLevel;
						player.UpdateSpellLineLevels(false);
						player.RefreshSpecDependantSkills(true);
						player.SaveIntoDatabase();
						player.Out.SendUpdatePlayerSkills();
					}
					break;
			}

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
				switch (item.TemplateID)
				{
					case "respec_cl":
						{
							player.Inventory.RemoveCountFromStack(item, 1);
							player.RespecAmountChampionSkill++;
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "CLWeaponNPC.ReceiveItem.RespecCL"), eChatType.CT_System, eChatLoc.CL_PopupWindow);
							return true;
						}
				}
			}

			return base.ReceiveItem(source, item);
		}
	}
	/*
		public class ThroneTeleporterNPC : GameNPC
		{
			public ThroneTeleporterNPC() : base()
			{
			}
			public override bool Interact(GamePlayer player)
			{
				if (!base.Interact(player))
					return false;
			
				TurnTo(player, 5000);
				if(!AmIInside())
					player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "ThroneTeleporterNPC.Interact.AskForAudience"),
													 this.Name, 
													 LanguageMgr.GetTranslation(player.Client, "GlobalWords.Yes"),
													 LanguageMgr.GetTranslation(player.Client, "GlobalWords.No")), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				else
					player.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(player.Client, "ThroneTeleporterNPC.Interact.AskForExit"),GetOutsideZone(player.Realm)), eChatType.CT_System, eChatLoc.CL_PopupWindow);
				return true;
			}
			public override bool WhisperReceive(GameLiving source, string str)
			{
				if (!base.WhisperReceive(source, str))
					return false;
            
				GamePlayer player = source as GamePlayer;
				if (player == null) return false;
 
				if(str.ToLower() == GetOutsideZone(player.Realm).ToLower())
            	
				if(str.ToLower() == LanguageMgr.GetTranslation(player.Client, "GlobalWords.Yes"))

				return true;
			}
			protected virtual bool TeleportPlayer(ushort pRegion)
			{
				player.MoveTo(location.Region, location.X, location.Y, location.Z, location.Heading);       	
			}
			protected virtual bool AmIInside()
			{
				// Check current region ID
				switch(this.CurrentRegionID)
				{
					case 394:
					case 360:
					case 395:
						return true;
					default:
						return false;
				}
			}
			protected virtual string GetOutsideZone(byte pRealm)
			{
				switch(pRealm)
				{
					case 1:
						return "Camelot";
					case 2:
						return "Jordheim";
					case 3:
						return "Tir na nog";
					default:
						return ""
				}
			}
		}
		*/
}
