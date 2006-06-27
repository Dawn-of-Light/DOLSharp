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
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Mystic Trainer
	/// </summary>	
	[NPCGuildScript("Mystic Trainer", eRealm.Midgard)]		// this attribute instructs DOL to use this script for all "Mystic Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MysticTrainer : GameTrainer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This hash constrain all item template the trainer can give
		/// </summary>	
		private static IDictionary allStartupItems = new Hashtable();

		/// <summary>
		/// This function is called at the server startup
		/// </summary>	
		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			#region Training staff

			StaffTemplate training_staff_template = new StaffTemplate();
			training_staff_template.Name = "training staff";
			training_staff_template.Level = 0;
			training_staff_template.Durability = 100;
			training_staff_template.Condition = 100;
			training_staff_template.Quality = 90;
			training_staff_template.Bonus = 0;
			training_staff_template.DamagePerSecond = 12;
			training_staff_template.Speed = 4500;
			training_staff_template.Weight = 45;
			training_staff_template.Model = 19;
			training_staff_template.Realm = eRealm.Midgard;
			training_staff_template.IsDropable = true; 
			training_staff_template.IsTradable = false; 
			training_staff_template.IsSaleable = false;
			training_staff_template.MaterialLevel = eMaterialLevel.Bronze;
				
			if(!allStartupItems.Contains("training_staff"))
			{
				allStartupItems.Add("training_staff", training_staff_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_staff_template.Name + " to MysticTrainer gifts.");
			}
			#endregion
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Mystic)
			{
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5)
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Runemaster] or [Spiritmaster]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByType("StaffTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice branch]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
			} 
			else
			{
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
			case "Runemaster":
				if(player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Kobold || player.Race == (int) eRace.Norseman || player.Race == (int) eRace.Dwarf){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Runemaster is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Spiritmaster":
				if(player.Race == (int) eRace.Kobold || player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Norseman){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Spiritmaster is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "practice branch":
				if (player.Inventory.GetFirstItemByType("StaffTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					GenericItemTemplate itemTemplate = allStartupItems["training_staff"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			
			}
			return true;			
		}
	}
}
