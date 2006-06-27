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
	/// Acolyte Trainer
	/// </summary>	
	[NPCGuildScript("Acolyte Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Acolyte Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class AcolyteTrainer : GameTrainer
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
			#region Practice mace

			CrushingWeaponTemplate training_mace_template = new CrushingWeaponTemplate();
			training_mace_template.Name = "practice mace";
			training_mace_template.Level = 0;
			training_mace_template.Durability = 100;
			training_mace_template.Condition = 100;
			training_mace_template.Quality = 90;
			training_mace_template.Bonus = 0;
			training_mace_template.DamagePerSecond = 12;
			training_mace_template.Speed = 2700;
			training_mace_template.Weight = 30;
			training_mace_template.Model = 13;
			training_mace_template.Realm = eRealm.Albion;
			training_mace_template.IsDropable = true; 
			training_mace_template.IsTradable = false; 
			training_mace_template.IsSaleable = false;
			training_mace_template.MaterialLevel = eMaterialLevel.Bronze;
				
			if(!allStartupItems.Contains("practice_mace"))
			{
				allStartupItems.Add("practice_mace", training_mace_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_mace_template.Name + " to AcolyteTrainer gifts.");
			}
			#endregion

			#region Training shield

			ShieldTemplate small_training_shield_template = new ShieldTemplate();
			small_training_shield_template.Name = "small training shield";
			small_training_shield_template.Level = 2;
			small_training_shield_template.Durability = 100;
			small_training_shield_template.Condition = 100;
			small_training_shield_template.Quality = 100;
			small_training_shield_template.Bonus = 0;
			small_training_shield_template.DamagePerSecond = 10;
			small_training_shield_template.Speed = 2000;
			small_training_shield_template.Size = eShieldSize.Small;
			small_training_shield_template.Weight = 32;
			small_training_shield_template.Model = 59;
			small_training_shield_template.Realm = eRealm.Albion;
			small_training_shield_template.IsDropable = true; 
			small_training_shield_template.IsTradable = false; 
			small_training_shield_template.IsSaleable = false;
			small_training_shield_template.MaterialLevel = eMaterialLevel.Bronze;
		
			if(!allStartupItems.Contains("small_training_shield"))
			{
				allStartupItems.Add("small_training_shield", small_training_shield_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + small_training_shield_template.Name + " to AcolyteTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Acolyte)
			{
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5) 
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Cleric] or [Friar]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
				

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByType("CrushingWeaponTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice weapon]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				if (player.Inventory.GetFirstItemByType("ShieldTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [training shield]?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
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
			case "practice weapon":
				if (player.Inventory.GetFirstItemByType("CrushingWeaponTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
				{
					GenericItemTemplate itemTemplate = allStartupItems["practice_mace"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			case "training shield":
				if (player.Inventory.GetFirstItemByType("ShieldTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) 
				{
					GenericItemTemplate itemTemplate = allStartupItems["small_training_shield"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			}
			return true;			
		}
	}
}
