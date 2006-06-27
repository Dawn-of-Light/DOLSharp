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
	/// Scout Trainer
	/// </summary>	
	[NPCGuildScript("Scout Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Scout Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ScoutTrainer : GameTrainer
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
			#region Scout longbow

			LongbowTemplate scout_item_template = new LongbowTemplate();
			scout_item_template.Name = "Huntsman's Longbow";
			scout_item_template.Level = 5;
			scout_item_template.Durability=100;
			scout_item_template.Condition = 100;
			scout_item_template.Quality = 90;
			scout_item_template.Bonus = 10;	
			scout_item_template.DamagePerSecond = 30;
			scout_item_template.Speed = 5400;
			scout_item_template.Weight = 31;
			scout_item_template.Model = 471;
			scout_item_template.Realm = eRealm.Albion;
			scout_item_template.IsDropable = true; 
			scout_item_template.IsTradable = false; 
			scout_item_template.IsSaleable = false;
			scout_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			scout_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Long_bows, 1));
				
			if(!allStartupItems.Contains("Huntsman_s_Longbow"))
			{
				allStartupItems.Add("Huntsman_s_Longbow", scout_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + scout_item_template.Name + " to ScoutTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Scout) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Defenders of Albion] and defend our realm as a Scout?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.AlbionRogue && (player.Race == (int) eRace.Briton
   				|| player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu));
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
			case "join the Defenders of Albion":
				// promote player to other class
				if (CanPromotePlayer(player)) 
					PromotePlayer(player, (int)eCharacterClass.Scout, "Welcome young warrior! May your time in Albion's army be rewarding! Here is your Huntsman's Longbow. The bow is your guild weapon! Be sure to carry it with you always.", new GenericItemTemplate[] {allStartupItems["Huntsman_s_Longbow"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
