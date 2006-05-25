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
using DOL.Database;
using DOL.Events;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Paladin Trainer
	/// </summary>	
	[NPCGuildScript("Paladin Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Paladin Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class PaladinTrainer : GameTrainer
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
			#region Slash weapon

			SlashingWeaponTemplate slash_sword_item_template = new SlashingWeaponTemplate();
			slash_sword_item_template.Name = "Sword of the Initiate";
			slash_sword_item_template.Level = 5;
			slash_sword_item_template.Durability = 100;
			slash_sword_item_template.Condition = 100;
			slash_sword_item_template.Quality = 90;
			slash_sword_item_template.Bonus = 10;
			slash_sword_item_template.DamagePerSecond = 30;
			slash_sword_item_template.Speed = 2500;
			slash_sword_item_template.Weight = 20;
			slash_sword_item_template.Model = 3;
			slash_sword_item_template.Realm = eRealm.Albion;
			slash_sword_item_template.IsDropable = true; 
			slash_sword_item_template.IsTradable = false; 
			slash_sword_item_template.IsSaleable = false;
			slash_sword_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			slash_sword_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Slashing, 1));
			
			if(!allStartupItems.Contains("Sword_of_the_Initiate"))
			{
				allStartupItems.Add("Sword_of_the_Initiate", slash_sword_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + slash_sword_item_template.Name + " to PaladinTrainer gifts.");
			}
			#endregion

			#region Thrust weapon

			ThrustWeaponTemplate thrust_sword_item_template = new ThrustWeaponTemplate();
			thrust_sword_item_template.Name = "Rapier of the Initiate";
			thrust_sword_item_template.Level = 5;
			thrust_sword_item_template.Durability = 100;
			thrust_sword_item_template.Condition = 100;
			thrust_sword_item_template.Quality = 90;
			thrust_sword_item_template.Bonus = 10;	
			thrust_sword_item_template.DamagePerSecond = 30;
			thrust_sword_item_template.Speed = 2500;
			thrust_sword_item_template.Weight = 10;
			thrust_sword_item_template.Model = 21;
			thrust_sword_item_template.Realm = eRealm.Albion;
			thrust_sword_item_template.IsDropable = true; 
			thrust_sword_item_template.IsTradable = false; 
			thrust_sword_item_template.IsSaleable = false;
			thrust_sword_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			thrust_sword_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Thrusting, 1));
			
			if(!allStartupItems.Contains("Rapier_of_the_Initiate"))
			{
				allStartupItems.Add("Rapier_of_the_Initiate", thrust_sword_item_template);
		
				if (log.IsDebugEnabled)
					log.Debug("Adding " + thrust_sword_item_template.Name + " to PaladinTrainer gifts.");
			}
			#endregion

			#region Crush weapon

			CrushingWeaponTemplate chrush_sword_item_template = new CrushingWeaponTemplate();
			chrush_sword_item_template.Name = "Mace of the Initiate";
			chrush_sword_item_template.Level = 5;
			chrush_sword_item_template.Durability = 100;
			chrush_sword_item_template.Condition = 100;
			chrush_sword_item_template.Quality = 90;
			chrush_sword_item_template.Bonus = 10;	
			chrush_sword_item_template.DamagePerSecond = 30;
			chrush_sword_item_template.Speed = 3000;
			chrush_sword_item_template.Weight = 32;
			chrush_sword_item_template.Model = 13;
			chrush_sword_item_template.Realm = eRealm.Albion;
			chrush_sword_item_template.IsDropable = true; 
			chrush_sword_item_template.IsTradable = false; 
			chrush_sword_item_template.IsSaleable = false;
			chrush_sword_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			chrush_sword_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Crushing, 1));
			
			if(!allStartupItems.Contains("Mace_of_the_Initiate"))
			{
				allStartupItems.Add("Mace_of_the_Initiate", chrush_sword_item_template);
	
				if (log.IsDebugEnabled)
					log.Debug("Adding " + chrush_sword_item_template.Name + " to PaladinTrainer gifts.");
			}
			#endregion

			#region Two handed weapon

			TwoHandedWeaponTemplate twohand_sword_item_template = new TwoHandedWeaponTemplate();
			twohand_sword_item_template.Name = "Greatsword of the Initiate";
			twohand_sword_item_template.Level = 5;
			twohand_sword_item_template.Durability = 100;
			twohand_sword_item_template.Condition = 100;
			twohand_sword_item_template.Quality = 90;
			twohand_sword_item_template.Bonus = 10;	
			twohand_sword_item_template.DamagePerSecond = 30;
			twohand_sword_item_template.Speed = 4700;
			twohand_sword_item_template.DamageType = eDamageType.Slash;
			twohand_sword_item_template.Weight = 55;
			twohand_sword_item_template.Model = 7;
			twohand_sword_item_template.Realm = eRealm.Albion;
			twohand_sword_item_template.IsDropable = true; 
			twohand_sword_item_template.IsTradable = false; 
			twohand_sword_item_template.IsSaleable = false;
			twohand_sword_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			twohand_sword_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Two_Handed, 1));
				
				
			if(!allStartupItems.Contains("Greatsword_of_the_Initiate"))
			{
				allStartupItems.Add("Greatsword_of_the_Initiate", twohand_sword_item_template);

				if (log.IsDebugEnabled)
					log.Debug("Adding " + twohand_sword_item_template.Name + " to PaladinTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Paladin"; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;
								
			// check if class matches.				
			if (player.CharacterClass.ID == (int) eCharacterClass.Paladin) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"The church has called out to you young warrior! Will you hear its calling and [join the Church of Albion]? Thus, walking the path of a Paladin forever?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				} else {
					player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);							
				}
			}
			return true;
 		}

		/// <summary>
		/// checks wether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Fighter && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian
				|| player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Saracen));
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
	
			
					if (CanPromotePlayer(player)) 
					{
					switch (text) 
						{
						case "join the Church of Albion":
								player.Out.SendMessage(this.Name + " says, \"Very well then! Choose your weapon, and your initiation into the Church of Albion will be complete. You may wield [slashing], [crushing], [thrusting] or [two handed] weapons.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
								break;
						case "slashing":
								PromotePlayer(player, (int)eCharacterClass.Paladin, "Here is your Sword of the Initiate. Welcome to the Church of Albion.", new GenericItemTemplate[] {allStartupItems["Sword_of_the_Initiate"] as GenericItemTemplate});
								break;
						case "crushing":
								PromotePlayer(player, (int)eCharacterClass.Paladin, "Here is your Mace of the Initiate. Welcome to the Church of Albion.", new GenericItemTemplate[] {allStartupItems["Mace_of_the_Initiate"] as GenericItemTemplate});
								break;
						case "thrusting":
								PromotePlayer(player, (int)eCharacterClass.Paladin, "Here is your Rapier of the Initiate. Welcome to the Church of Albion.", new GenericItemTemplate[] {allStartupItems["Rapier_of_the_Initiate"] as GenericItemTemplate});
								break;
						case "two handed":
								PromotePlayer(player, (int)eCharacterClass.Paladin, "Here is your Great Sword of the Initiate. Welcome to the Church of Albion.", new GenericItemTemplate[] {allStartupItems["Greatsword_of_the_Initiate"] as GenericItemTemplate});
								break;
					}
			}
			return true;		
		}
	}
}
