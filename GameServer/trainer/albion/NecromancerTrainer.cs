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
	/// Necromancer Trainer
	/// </summary>	
	[NPCGuildScript("Necromancer Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Necromancer Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class NecromancerTrainer : GameTrainer
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
			#region Necromancer staff

			StaffTemplate necromancer_item_template = new StaffTemplate();
			necromancer_item_template.Name = "Necromancer's Staff";
			necromancer_item_template.Level = 5;
			necromancer_item_template.Durability=100;
			necromancer_item_template.Condition = 100;
			necromancer_item_template.Quality = 90;
			necromancer_item_template.Bonus = 10;
			necromancer_item_template.DamagePerSecond = 30;
			necromancer_item_template.Speed = 4400;
			necromancer_item_template.Weight = 45;
			necromancer_item_template.Model = 821;
			necromancer_item_template.Realm = eRealm.Albion;
			necromancer_item_template.IsDropable = true; 
			necromancer_item_template.IsTradable = false; 
			necromancer_item_template.IsSaleable = false;
			necromancer_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			necromancer_item_template.AllowedClass.Add(eCharacterClass.Necromancer);

			necromancer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_DeathSight, 4));
			necromancer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_PainWorking, 4));
			necromancer_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_DeathServant, 4));
				
				
			if(!allStartupItems.Contains("Necromancer_s_Staff"))
			{
				allStartupItems.Add("Necromancer_s_Staff", necromancer_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + necromancer_item_template.Name + " to NecromancerTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Necromancer"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Necromancer) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Temple of Arawn] and defend our realm as a Necromancer?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Disciple && (player.Race == (int) eRace.Briton
   				|| player.Race == (int) eRace.Saracen || player.Race == (int) eRace.Inconnu));
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
			case "join the Temple of Arawn":
				// promote player to other class
				if (CanPromotePlayer(player)) 
					PromotePlayer(player, (int)eCharacterClass.Necromancer, "Lord Arawn has accepted you into his Temple. Here is his gift to you. Use it well, Disciple.", new GenericItemTemplate[] {allStartupItems["Necromancer_s_Staff"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
