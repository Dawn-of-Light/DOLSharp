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
	/// Mage Trainer
	/// </summary>	
	[NPCGuildScript("Mage Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Mage Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MageTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Mage; }
		}

		public const string PRACTICE_WEAPON_ID = "trimmed_branch";
		
		public MageTrainer() : base((int)CLTrainerTypes.Mage)
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Mage)
			{

				// popup the training window
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5)
				{
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Cabalist] or [Sorcerer]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
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
			case "Cabalist":
				if(player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.Briton || player.Race == (int) eRace.HalfOgre || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"So, you seek to embrace a darker side of magic do you? A Cabalist craving to summon up Golems out of inanimate matter is a true asset to Albion. Yet, because of their thirst for greater power many will not teach this skill. Therefore should one wish to follow this path; they must seek out the Guild of Shadows.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Cabalist is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Sorcerer":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.HalfOgre || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"So you wish to focus your training towards a pragmatic magic. Sorcerers prove their worth to The Academy by summoning up spells that disrupt, disable, and damage their enemies. In time you may even conjure up beasts to do your bidding.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Sorcerer is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
