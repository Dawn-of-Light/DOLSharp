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
	/// Sorcerer Trainer
	/// </summary>	
	[NPCGuildScript("Sorcerer Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Sorcerer Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class SorcererTrainer : GameTrainer
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
			#region Sorcerer staff

			StaffTemplate sorcerer_item_template = new StaffTemplate();
			sorcerer_item_template.Name = "Sorcerer Staff of Focus";
			sorcerer_item_template.Level = 5;
			sorcerer_item_template.Durability=100;
			sorcerer_item_template.Condition = 100;
			sorcerer_item_template.Quality = 90;
			sorcerer_item_template.Bonus = 10;
			sorcerer_item_template.DamagePerSecond = 30;
			sorcerer_item_template.Speed = 4400;
			sorcerer_item_template.Weight = 45;
			sorcerer_item_template.Model = 19;
			sorcerer_item_template.Realm = eRealm.Albion;
			sorcerer_item_template.IsDropable = true; 
			sorcerer_item_template.IsTradable = false; 
			sorcerer_item_template.IsSaleable = false;
			sorcerer_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			sorcerer_item_template.AllowedClass.Add(eCharacterClass.Sorcerer);

			sorcerer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Matter, 4));
			sorcerer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Body, 4));
			sorcerer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Mind, 4));
			
			if(!allStartupItems.Contains("Sorcerer_Staff_of_Focus"))
			{
				allStartupItems.Add("Sorcerer_Staff_of_Focus", sorcerer_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + sorcerer_item_template.Name + " to SorcererTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Sorcerer"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Sorcerer) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Is it your wish to [join the Academy] and lend us your power as a Sorcerer?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Mage && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian
				|| player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.HalfOgre));
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
			case "join the Academy":
				// promote player to other class
				if (CanPromotePlayer(player)) 
					PromotePlayer(player, (int)eCharacterClass.Sorcerer, "You are now part of our shadow! You shall forever have a place among us! Here too is your guild weapon, a Staff of Focus!", new GenericItemTemplate[] {allStartupItems["Sorcerer_Staff_of_Focus"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
