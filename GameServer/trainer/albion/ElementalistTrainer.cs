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
	/// Elementalist Trainer
	/// </summary>	
	[NPCGuildScript("Elementalist Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Elementalist Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ElementalistTrainer : GameTrainer
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
			#region Trimmed branch

			StaffTemplate trimmed_branch_template = new StaffTemplate();
			trimmed_branch_template.Name = "trimmed branch";
			trimmed_branch_template.Level = 0;
			trimmed_branch_template.Durability = 100;
			trimmed_branch_template.Condition = 100;
			trimmed_branch_template.Quality = 90;
			trimmed_branch_template.Bonus = 0;
			trimmed_branch_template.DamagePerSecond = 12;
			trimmed_branch_template.Speed = 2700;
			trimmed_branch_template.Weight = 12;
			trimmed_branch_template.Model = 19;
			trimmed_branch_template.Realm = eRealm.Albion;
			trimmed_branch_template.IsDropable = true; 
			trimmed_branch_template.IsTradable = false; 
			trimmed_branch_template.IsSaleable = false;
			trimmed_branch_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains("trimmed_branch"))
			{
				allStartupItems.Add("trimmed_branch", trimmed_branch_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + trimmed_branch_template.Name + " to ElementalistTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Elementalist) 
			{
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5) 
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Theurgist] or [Wizard]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
				

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByType("StaffTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice weapon]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			case "Theurgist":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.HalfOgre){
					player.Out.SendMessage(this.Name + " says, \"You wish to study the art of magical enchantments do you? The Defenders of Albion rely immensely on this ability and their art of building and animating creatures that can fight and protect the army while in battle.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Theurgist is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Wizard":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.HalfOgre){
					player.Out.SendMessage(this.Name + " says, \"I see you wish to specialize in molding the four elements of fire, ice, earth, and air to create magical spells of immense power. Even now many of The Academy well-trained Wizards rain destruction upon our enemies.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Wizard is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "practice weapon":
				if (player.Inventory.GetFirstItemByType("StaffTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					GenericItemTemplate itemTemplate = allStartupItems["trimmed_branch"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			}
			return true;			
		}
	}
}
