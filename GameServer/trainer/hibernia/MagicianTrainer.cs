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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Magician Trainer
	/// </summary>
	[NPCGuildScript("Magician Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Magician Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MagicianTrainer : GameTrainer
	{
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Magician; }
		}

		public const string PRACTICE_WEAPON_ID = "training_staff";
		
		public MagicianTrainer() : base(eChampionTrainerType.Magician)
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
			if (player.CharacterClass.ID == (int) TrainedClass)
			{
				// player can be promoted
				if (player.Level>=5)
				{
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Eldritch], [Enchanter] or [Mentalist]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else
				{
					OfferTraining(player);
				}

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByID(PRACTICE_WEAPON_ID, eInventorySlot.MinEquipable, eInventorySlot.LastBackpack) == null)
				{
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice staff]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
				case "Eldritch":
					if(player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen){
						player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					else{
						player.Out.SendMessage(this.Name + " says, \"The path of a Eldritch is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					return true;
				case "Enchanter":
					if(player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen){
						player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					else{
						player.Out.SendMessage(this.Name + " says, \"The path of a Enchanter is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					return true;
				case "Mentalist":
					if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar)
					{
						player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					else
					{
						player.Out.SendMessage(this.Name + " says, \"The path of a Mentalist is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
					}
					return true;
				case "practice staff":
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
