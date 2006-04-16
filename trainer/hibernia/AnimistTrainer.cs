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
	/// Animist Trainer
	/// </summary>	
	[NPCGuildScript("Animist Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Animist Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class AnimistTrainer : GameTrainer
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
			#region Animist staff

			StaffTemplate animist_staff_template = new StaffTemplate();
			animist_staff_template.Name = "Staff of the Aborical";
			animist_staff_template.Level = 5;
			animist_staff_template.Durability=100;
			animist_staff_template.Condition = 100;
			animist_staff_template.Quality = 90;
			animist_staff_template.Bonus = 10;
			animist_staff_template.DamagePerSecond = 30;
			animist_staff_template.Speed = 4400;
			animist_staff_template.Weight = 45;
			animist_staff_template.Model = 19;
			animist_staff_template.Realm = eRealm.Hibernia;
			animist_staff_template.IsDropable = true; 
			animist_staff_template.IsTradable = false; 
			animist_staff_template.IsSaleable = false;
			animist_staff_template.MaterialLevel = eMaterialLevel.Bronze;
			
			animist_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Arboreal, 4));
			animist_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_CreepingPath, 4));
			animist_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Verdant, 4));
			
			if(!allStartupItems.Contains("Staff_of_the_Aborical"))
			{
				allStartupItems.Add("Staff_of_the_Aborical", animist_staff_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + animist_staff_template.Name + " to AnimistTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Animist"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Animist)
			{
				player.Out.SendTrainerWindow();
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Is the [Path of Affinity] the path you desire to walk?\"",eChatType.CT_System,eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Forester && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg
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
				case "Path of Affinity":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Animist, "You are now an Animist, " + source.GetName(0, false) + ". Welcome to the Path of Affinity.", new GenericItemTemplate[] {allStartupItems["Staff_of_the_Aborical"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
