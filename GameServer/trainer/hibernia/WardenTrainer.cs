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
	/// Warden Trainer
	/// </summary>	
	[NPCGuildScript("Warden Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Warden Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class WardenTrainer : GameTrainer
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
			#region Warden boots

			FeetArmorTemplate warden_boots_template = new FeetArmorTemplate();
			warden_boots_template.Name = "Warden Initiate Boots";
			warden_boots_template.Level = 5;
			warden_boots_template.Durability = 100;
			warden_boots_template.Condition = 100;
			warden_boots_template.Quality = 90;
			warden_boots_template.Bonus = 10;
			warden_boots_template.ArmorLevel = eArmorLevel.Low;
			warden_boots_template.ArmorFactor = 14;
			warden_boots_template.Weight = 16;
			warden_boots_template.Model = 357;
			warden_boots_template.Realm = eRealm.Hibernia;
			warden_boots_template.IsDropable = true; 
			warden_boots_template.IsTradable = false; 
			warden_boots_template.IsSaleable = false;
			warden_boots_template.MaterialLevel = eMaterialLevel.Bronze;
			
			warden_boots_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Quickness, 1));
			
			if(!allStartupItems.Contains("Warden_Initiate_Boots"))
			{
				allStartupItems.Add("Warden_Initiate_Boots", warden_boots_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + warden_boots_template.Name + " to WardenTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Warden"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Warden)
			{
				player.Out.SendTrainerWindow();
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Nidewst! You wish to follow the Path of Focus and train as a [Warden]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
			} 
			else 
			{
				player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			}
			return true;
 		}

		/// <summary>
		/// checks whether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanPromotePlayer(GamePlayer player) 
		{
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Naturalist && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg
				|| player.Race == (int) eRace.Sylvan));
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
	
			switch (text) 
			{
				case "Warden":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Warden, "Good then! Welcome to the ways of the Warden! Here, take this as a gift, to start you on the path of a Warden.", new GenericItemTemplate[] {allStartupItems["Warden_Initiate_Boots"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
