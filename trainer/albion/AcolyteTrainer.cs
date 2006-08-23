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

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Acolyte Trainer
	/// </summary>	
	[NPCGuildScript("Acolyte Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Acolyte Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class AcolyteTrainer : GameTrainer
	{
		public const string PRACTICE_WEAPON_ID = "training_mace";
		public const string PRACTICE_SHIELD_ID = "small_training_shield";

		public AcolyteTrainer() : base()
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
								
			// check if class matches				
			if (player.CharacterClass.ID == (int) eCharacterClass.Acolyte) {

				// popup the training window
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5) {
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Cleric], [Heretic], or [Friar]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				} else {
					//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				}

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice weapon]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				if (player.Inventory.GetFirstItemByID(PRACTICE_SHIELD_ID, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [training shield]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
			} else {
				player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
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

			switch (text) {
			case "Cleric":
				if(player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.Briton || player.Race == (int) eRace.Highlander){
					this.SayTo(player,"So, you wish to serve the Church as healer, defender and leader of our faith. The Church of Albion will welcome one of your skill. Perhaps in time, your commitment will lead others to join our order.");
				}
				else{
					this.SayTo(player,"The path of a Cleric is not available to your race. Please choose another.");
				}
				return true;
			case "Friar":
				if(player.Race == (int) eRace.Briton){
					this.SayTo(player,"Members of a brotherhood, you will find more than a community should you join ranks with the Defenders of Albion. Deadly with a Quarterstaff, and proficient with the healing of wounds, the army is in constant need of new recruits such as you.");
				}
				else{
					this.SayTo(player,"The path of a Friar is not available to your race. Please choose another.");
				}
				return true;
			case "Heretec":
			if(player.Race == (int)eRace.Briton || player.Race == (int)eRace.Avalonian || player.Race == (int)eRace.Inconnu || player.Race == (int)eRace.AlbionMinotaur)
				{
					this.SayTo(player, "Members of a brotherhood, you will find more than a community should you join ranks with the Defenders of Albion. Deadly with a Quarterstaff, and proficient with the healing of wounds, the army is in constant need of new recruits such as you.");
				}
				else
				{
					this.SayTo(player, "The path of a Friar is not available to your race. Please choose another.");
				}
				return true;
			case "practice weapon":
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
				{
					player.ReceiveItem(this,PRACTICE_WEAPON_ID);
				}
				return true;
			case "training shield":
				if (player.Inventory.GetFirstItemByID(PRACTICE_SHIELD_ID, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
				{
					player.ReceiveItem(this,PRACTICE_SHIELD_ID); 
				}
				return true;
			}
			return true;			
		}
	}
}
