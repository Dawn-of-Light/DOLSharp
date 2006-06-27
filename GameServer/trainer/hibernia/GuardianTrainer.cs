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
	/// Guardian Trainer
	/// </summary>	
	[NPCGuildScript("Guardian Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Guardian Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class GuardianTrainer : GameTrainer
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
			#region Training sword

			BladesTemplate training_sword_hib_template = new BladesTemplate();
			training_sword_hib_template.Name = "training sword";
			training_sword_hib_template.Level = 0;
			training_sword_hib_template.Durability = 100;
			training_sword_hib_template.Condition = 100;
			training_sword_hib_template.Quality = 90;
			training_sword_hib_template.Bonus = 0;
			training_sword_hib_template.DamagePerSecond = 12;
			training_sword_hib_template.Speed = 2500;
			training_sword_hib_template.HandNeeded = eHandNeeded.LeftHand;
			training_sword_hib_template.Weight = 18;
			training_sword_hib_template.Model = 445;
			training_sword_hib_template.Realm = eRealm.Hibernia;
			training_sword_hib_template.IsDropable = true; 
			training_sword_hib_template.IsTradable = false; 
			training_sword_hib_template.IsSaleable = false;
			training_sword_hib_template.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains("training_sword"))
			{
				allStartupItems.Add("training_sword", training_sword_hib_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_sword_hib_template.Name + " to GuardianTrainer gifts.");
			}
			#endregion

			#region Training shield

			ShieldTemplate training_shield_template_hib = new ShieldTemplate();
			training_shield_template_hib.Name = "training shield";
			training_shield_template_hib.Level = 2;
			training_shield_template_hib.Durability = 100;
			training_shield_template_hib.Condition = 100;
			training_shield_template_hib.Quality = 90;
			training_shield_template_hib.Bonus = 0;
			training_shield_template_hib.DamagePerSecond = 10;
			training_shield_template_hib.Speed = 2000;
			training_shield_template_hib.Size = eShieldSize.Small;
			training_shield_template_hib.Weight = 32;
			training_shield_template_hib.Model = 59;
			training_shield_template_hib.Realm = eRealm.Hibernia;
			training_shield_template_hib.IsDropable = true; 
			training_shield_template_hib.IsTradable = false; 
			training_shield_template_hib.IsSaleable = false;
			training_shield_template_hib.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains("training_shield"))
			{
				allStartupItems.Add("training_shield", training_shield_template_hib);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_shield_template_hib.Name + " to GuardianTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Guardian)
			{
				player.Out.SendTrainerWindow();
							
				// player can be promoted
				if (player.Level>=5) 
					player.Out.SendMessage(this.Name + " says, \"You must now seek your training elsewhere. Which path would you like to follow? [Hero], [Champion] or [Blademaster]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				

				// ask for basic equipment if player doesnt own it
				if (player.Inventory.GetFirstItemByType("BladesTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [practice weapon]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				if (player.Inventory.GetFirstItemByType("ShieldTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null) {
					player.Out.SendMessage(this.Name + " says, \"Do you require a [training shield]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			case "Hero":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg || player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar || player.Race == (int) eRace.Sylvan){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Hero is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Champion":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Champion is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Blademaster":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf || player.Race == (int) eRace.Firbolg || player.Race == (int) eRace.Shar){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Blademaster is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "practice weapon":
				if (player.Inventory.GetFirstItemByType("BladesTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					GenericItemTemplate itemTemplate = allStartupItems["training_sword"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			case "training shield":
				if (player.Inventory.GetFirstItemByType("ShieldTemplate", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					GenericItemTemplate itemTemplate = allStartupItems["training_shield"] as GenericItemTemplate;
					if(itemTemplate != null)
						player.ReceiveItem(this, itemTemplate.CreateInstance());
				}
				return true;
			}
			return true;			
		}
	}
}
