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
	/// Cleric Trainer
	/// </summary>	
	[NPCGuildScript("Cleric Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Cleric Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ClericTrainer : GameTrainer
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
			
			chrush_sword_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Rejuvenation, 1));
			
			if(!allStartupItems.Contains("Mace_of_the_Initiate"))
			{
				allStartupItems.Add("Mace_of_the_Initiate", chrush_sword_item_template);
	
				if (log.IsDebugEnabled)
					log.Debug("Adding " + chrush_sword_item_template.Name + " to ClericTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Cleric"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Cleric) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Church of Albion] and walk the path of a Cleric?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Acolyte && (player.Race == (int) eRace.Briton || player.Race == (int) eRace.Avalonian
				|| player.Race == (int) eRace.Highlander));
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
			case "join the Church of Albion":
				// promote player to other class
				if (CanPromotePlayer(player)) 
					PromotePlayer(player, (int)eCharacterClass.Cleric, "Welcome my child! Walk the path of light, shout to all the words of our beloved church and rid the land of the faithless! Here is your Mace of the Initiate. It is our standard gift to all new members.", new GenericItemTemplate[] {allStartupItems["Mace_of_the_Initiate"] as GenericItemTemplate});
				
				break;
			}
			return true;		
		}
	}
}
