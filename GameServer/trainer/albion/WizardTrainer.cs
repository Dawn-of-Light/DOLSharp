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
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Wizard Trainer
	/// </summary>	
	[NPCGuildScript("Wizard Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Wizard Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class WizardTrainer : GameTrainer
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
			#region Wizard staff

			StaffTemplate wizard_item_template = new StaffTemplate();
			wizard_item_template.Name = "Wizard Staff of Focus";
			wizard_item_template.Level = 5;
			wizard_item_template.Durability=100;
			wizard_item_template.Condition = 100;
			wizard_item_template.Quality = 90;
			wizard_item_template.Bonus = 10;
			wizard_item_template.DamagePerSecond = 30;
			wizard_item_template.Speed = 4400;
			wizard_item_template.Weight = 45;
			wizard_item_template.Model = 19;
			wizard_item_template.Realm = eRealm.Albion;
			wizard_item_template.IsDropable = true; 
			wizard_item_template.IsTradable = false; 
			wizard_item_template.IsSaleable = false;
			wizard_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			wizard_item_template.AllowedClass.Add(eCharacterClass.Wizard);

			wizard_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Earth, 4));
			wizard_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Cold, 4));
			wizard_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Fire, 4));
				
			if(!allStartupItems.Contains("Wizard_Staff_of_Focus"))
			{
				allStartupItems.Add("Wizard_Staff_of_Focus", wizard_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + wizard_item_template.Name + " to WizardTrainer gifts.");
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
								
			// check if class matches.				
			if (player.CharacterClass.ID == (int) eCharacterClass.Wizard) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Academy] and summon the power of the elements as a Wizard?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Elementalist && (player.Race == (int) eRace.Briton
				|| player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.HalfOgre));
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
					PromotePlayer(player, (int)eCharacterClass.Wizard, "Welcome to our guild! You have much to learn, but I see greatness in your future! Here too is your guild weapon, a Staff of Focus!", new GenericItemTemplate[] {allStartupItems["Wizard_Staff_of_Focus"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
