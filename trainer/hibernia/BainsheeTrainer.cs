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
using DOL.Language;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Bainshee Trainer
	/// </summary>
	[NPCGuildScript("Bainshee Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Bainshee Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class BainsheeTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Bainshee; }
		}

		public const string WEAPON_ID1 = "bainshee_item";

		public BainsheeTrainer()
			: base()
		{
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;

			// check if class matches.
			if (player.CharacterClass.ID == (int) TrainedClass)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "BainsheeTrainer.Interact.Text2", this.Name, player.Name), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}
			else
			{
				// perhaps player can be promoted
				if (CanPromotePlayer(player))
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "BainsheeTrainer.Interact.Text1", this.Name, player.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
					if (!player.IsLevelRespecUsed)
					{
						OfferRespecialize(player);
					}
				}
				else
				{
					CheckChampionTraining(player);
				}
			}
			return true;
		}

		/// <summary>
		/// checks whether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level >= 5 && player.DBCharacter.Gender == 1 && player.CharacterClass.ID == (int)eCharacterClass.Magician && (player.Race == (int)eRace.Celt || player.Race == (int)eRace.Elf
			                                                                                                                             || player.Race == (int)eRace.Lurikeen));
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
			String lowerCase = text.ToLower();

			if (lowerCase == LanguageMgr.GetTranslation(player.Client, "BainsheeTrainer.WhisperReceiveCase.Text1"))
			{
				// promote player to other class
				if (CanPromotePlayer(player))
				{
					player.RemoveAllSpellLines();
					player.RemoveAllSkills();
					player.RemoveAllSpecs();
					player.RemoveAllStyles();
					player.Out.SendUpdatePlayerSkills();
					player.SkillSpecialtyPoints = 14;//lvl 5 skill points full
					PromotePlayer(player, (int)eCharacterClass.Bainshee, LanguageMgr.GetTranslation(player.Client, "BainsheeTrainer.WhisperReceive.Text1", player.GetName(0, false)), null);
					player.ReceiveItem(this, WEAPON_ID1);
				}
			}
			return true;
		}
	}
}
