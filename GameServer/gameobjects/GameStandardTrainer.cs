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
using System.Collections;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// The mother class for all class trainers
	/// </summary>
	public abstract class GameStandardTrainer : GameTrainer
	{
		/// <summary>
		/// Gets trained class
		/// </summary>
		public abstract eCharacterClass TrainedClass
		{
			get;
		}

		/// <summary>
		/// Gets all trainer gifts
		/// </summary>
		public abstract IList TrainerGifts
		{
			get;
		}

		#region GetExamineMessages / Interact
		
		/// <summary>
		/// checks wether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanPromotePlayer(GamePlayer player) 
		{
			return player.Level >= 5;
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
				player.Out.SendTrainerWindow();
				
				// player can be promoted
				if (CanPromotePlayer(player))
				{
					player.Out.SendMessage(Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
					return true;
				}
				
				foreach(GenericItemTemplate itemTemplate in TrainerGifts)
				{
					// ask for basic equipment if player doesnt own it
					if (player.Inventory.GetFirstItemByName(itemTemplate.Name, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
					{
						string itemName = "practice weapon";
						if(itemTemplate is ShieldTemplate) itemName = "training shield";
						else if(itemTemplate is StaffTemplate) itemName = "practice branch";

						player.Out.SendMessage(Name + " says, \"Do you require a ["+itemName+"]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
					}
				}
			}
			else
			{
				player.Out.SendMessage(Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}

			return false;
		}
		#endregion

		/// <summary>
		/// Talk to trainer
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text)) return false;

			if(text == "training shield" || text == "practice weapon" || text == "practice branch")
			{
				GenericItem itemToAdd = null;
				foreach(GenericItemTemplate itemTemplate in TrainerGifts)
				{
					if(text == "training shield" && itemTemplate is ShieldTemplate
					|| text == "practice branch" && itemTemplate is StaffTemplate
					|| text == "practice weapon" && !(itemTemplate is ShieldTemplate) && !(itemTemplate is StaffTemplate))
					{
						itemToAdd = itemTemplate.CreateInstance();
						break;
					}
				}
				if(itemToAdd != null && source.Inventory.GetFirstItemByName(itemToAdd.Name, eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					source.ReceiveItem(this, itemToAdd);
				}
			}
			return true;
		}
	}
}
