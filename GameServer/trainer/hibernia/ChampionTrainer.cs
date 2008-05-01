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
	/// Champion Trainer
	/// </summary>	
	[NPCGuildScript("Champion Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Champion Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ChampionTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Champion; }
		}

		public const string ARMOR_ID1 = "champion_item";

		public ChampionTrainer() : base()
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Champion) 
			{
				// popup the training window
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"I'm glad to see you taking an interest in your training, " + player.Name + ". There is always room to grow and learn!\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else 
			{
				// perhaps player can be promoted
				if (CanPromotePlayer(player))
				{
					player.Out.SendMessage(this.Name + " says, \"Champions follow the Path of Essence. Choose now to become a [Champion], and I will train you in our ways, and the ways of the Path we follow.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Guardian && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf
				|| player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar));
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
	
			switch (text) {
			case "Champion":
				// promote player to other class
				if (CanPromotePlayer(player)) {
					PromotePlayer(player, (int)eCharacterClass.Champion, "Welcome " + source.GetName(0, false) + ". Let us see if you will become a worthy Champion. Take this gift, " + source.GetName(0, false) + ". It is to aid you while you grow into a true Champion.", null);
					player.ReceiveItem(this,ARMOR_ID1);
				}
				break;
			}
			return true;		
		}
	}
}
