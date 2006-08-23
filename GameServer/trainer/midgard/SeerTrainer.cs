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
	/// Seer Trainer
	/// </summary>	
	[NPCGuildScript("Seer Trainer", eRealm.Midgard)]		// this attribute instructs DOL to use this script for all "Acolyte Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class SeerTrainer : GameTrainer
	{
		public const string PRACTICE_WEAPON_ID = "training_hammer";
		public const string PRACTICE_SHIELD_ID = "small_training_shield";

		public SeerTrainer() : base()
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Seer) {

				// popup the training window
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5) {
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Shaman] or [Healer]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
			case "Shaman":
				if(player.Race == (int)eRace.Frostalf || player.Race == (int)eRace.Kobold || player.Race == (int)eRace.Troll || player.Race == (int)eRace.MidgardMinotaur)
				{
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Shaman is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Healer":
				if(player.Race == (int) eRace.Dwarf || player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Norseman){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Healer is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
