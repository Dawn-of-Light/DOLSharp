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
using DOL.Database2;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Albion Rogue Trainer
	/// </summary>	
	[NPCGuildScript("Rogue Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Rogue Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class AlbionRogueTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.AlbionRogue; }
		}

		public const string PRACTICE_WEAPON_ID = "practice_dirk";
	
		public AlbionRogueTrainer() : base((int)CLTrainerTypes.AlbionRogue)
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
			if (player.CharacterClass.ID == (int) eCharacterClass.AlbionRogue)
			{

				// popup the training window
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5)
				{
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Infiltrator], [Minstrel], or [Scout]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
				}
				else
				{
					//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
				}

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, eInventorySlot.MinEquipable, eInventorySlot.LastBackpack) == null)
				{
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice weapon]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
			}
			else
			{
				DismissPlayer(player);
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
			case "Infiltrator":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu){
					player.Out.SendMessage(this.Name + " says, \"You seek a tough life if you go that path. The life of an Infiltrator involves daily use of his special skills. The Guild of Shadows has made its fortune by using them to sneak, hide, disguise, backstab, and spy on the enemy. Without question they are an invaluable asset to Albion.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of an Infiltrator is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Minstrel":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Highlander){
					player.Out.SendMessage(this.Name + " says, \"Ah! To sing the victories of Albion! To give praise to those who fight to keep the light of Camelot eternal. Many have studied their skill within the walls of The Academy and come forth to defend this realm. Without their magical songs, many would not be here today.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Minstrel is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Scout":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Inconnu){
					player.Out.SendMessage(this.Name + " says, \"Ah! You wish to join the Defenders of Albion eh? That is quite a good choice for a Rogue. A Scouts accuracy with a bow is feared by all our enemies and has won Albion quite a few battles.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Scout is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "practice weapon":
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
				{
					player.ReceiveItem(this,PRACTICE_WEAPON_ID);
				}
				return true;
			}
			return true;			
		}
	}
}
