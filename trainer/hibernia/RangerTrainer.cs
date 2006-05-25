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
	/// Ranger Trainer
	/// </summary>	
	[NPCGuildScript("Ranger Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Ranger Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class RangerTrainer : GameTrainer
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
			#region Ranger Recurve Bow

			RecurvedbowTemplate ranger_item_template = new RecurvedbowTemplate();
			ranger_item_template.Name = "Elven Recurve Bow";
			ranger_item_template.Level = 5;
			ranger_item_template.Durability=100;
			ranger_item_template.Condition = 100;
			ranger_item_template.Quality = 90;
			ranger_item_template.Bonus = 10;	
			ranger_item_template.DamagePerSecond = 30;
			ranger_item_template.Speed = 4700;
			ranger_item_template.Weight = 31;
			ranger_item_template.Model = 471;
			ranger_item_template.Realm = eRealm.Hibernia;
			ranger_item_template.IsDropable = true; 
			ranger_item_template.IsTradable = false; 
			ranger_item_template.IsSaleable = false;
			ranger_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			ranger_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_RecurvedBow, 1));
			
			if(!allStartupItems.Contains("Elven_Recurve_Bow"))
			{
				allStartupItems.Add("Elven_Recurve_Bow", ranger_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + ranger_item_template.Name + " to RangerTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Ranger"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Ranger)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"You wish to learn more of our ways? Fine then.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"You know, the way of a [Ranger] is not for everyone. Are you sure this is your choice?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Stalker && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf
				|| player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar));
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
				case "Ranger":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Ranger, "Good then. Your path as a Ranger is before you. Walk it with care, friend. Take these, " + source.GetName(0, false) + ", to help make walking the path a bit easier.", new GenericItemTemplate[] {allStartupItems["Elven_Recurve_Bow"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
