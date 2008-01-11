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

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Mercenary Trainer
	/// </summary>	
	[NPCGuildScript("Mercenary Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Mercenary Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MercenaryTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Mercenary; }
		}

		public const string WEAPON_ID1 = "slash_sword_item";
		public const string WEAPON_ID2 = "crush_sword_item";
		public const string WEAPON_ID3 = "thrust_sword_item";

		public MercenaryTrainer() : base()
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Mercenary) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you wish to [join the Guild of Shadows] and seek your fortune as a Mercenary?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				} else {
					player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);							
				}
			}
			return true;
 		}

		/// <summary>
		/// checks wether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Fighter && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian
				|| player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.HalfOgre || player.Race == (int) eRace.Inconnu
				|| player.Race == (int) eRace.AlbionMinotaur));
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
	

			if (CanPromotePlayer(player)) 
			{
				switch (text) 
				{
					case "join the Guild of Shadows":
						player.Out.SendMessage(this.Name + " says, \"Very well. Choose a weapon, and you shall become one of us. Which would you have, [slashing], [crushing], or [thrusting]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
						break;
					case "slashing":
					
						PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Sword of the Initiate. Welcome to the Guild of Shadows.", null);
						player.ReceiveItem(this,WEAPON_ID1);
					
						break;
					case "crushing":
					
						PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Mace of the Initiate. Welcome to the Guild of Shadows.", null);
						player.ReceiveItem(this,WEAPON_ID2);
					
						break;
					case "thrusting":
					
						PromotePlayer(player, (int)eCharacterClass.Mercenary, "Here is your Rapier of the Initiate. Welcome to the Guild of Shadows.", null);
						player.ReceiveItem(this,WEAPON_ID3);
					
						break;
				}
			}
			return true;		
		}
	}
}
