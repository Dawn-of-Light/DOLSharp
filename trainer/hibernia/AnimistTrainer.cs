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
	/// Animist Trainer
	/// </summary>	
	[NPCGuildScript("Animist Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Animist Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class AnimistTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Animist; }
		}

		public const string WEAPON_ID1 = "animist_item";

		public AnimistTrainer() : base()
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Animist)
			{
				// popup the training window
				player.Out.SendTrainerWindow();
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AnimistTrainer.Interact.Text2", this.Name, player.GetName(0, false)), eChatType.CT_Say, eChatLoc.CL_ChatWindow);

			} 
			else 
			{
				// perhaps player can be promoted
				if (CanPromotePlayer(player))
				{
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AnimistTrainer.Interact.Text1", this.Name), eChatType.CT_System, eChatLoc.CL_PopupWindow);
					if (!player.IsLevelRespecUsed)
					{
						OfferRespecialize(player);
					}
				}
				else
				{
					DismissPlayer(player);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Forester && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg
				|| player.Race == (int) eRace.Sylvan));
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

            if (lowerCase == LanguageMgr.GetTranslation(player.Client, "AnimistTrainer.WhisperReceiveCase.Text1"))
			{
				// promote player to other class
				if (CanPromotePlayer(player))
				{
                    PromotePlayer(player, (int)eCharacterClass.Animist, LanguageMgr.GetTranslation(player.Client, "AnimistTrainer.WhisperReceive.Text1", player.GetName(0, false)), null);
					player.ReceiveItem(this, WEAPON_ID1);
				}
			}
			return true;		
		}
	}
}
