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
	/// Eldritch Trainer
	/// </summary>	
	[NPCGuildScript("Eldritch Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Eldritch Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class EldritchTrainer : GameTrainer
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
			#region Eldritch staff

			StaffTemplate eldritch_staff_template = new StaffTemplate();
			eldritch_staff_template.Name = "Eldritch Staff of Focus";
			eldritch_staff_template.Level = 5;
			eldritch_staff_template.Durability = 100;
			eldritch_staff_template.Condition = 100;
			eldritch_staff_template.Quality = 90;
			eldritch_staff_template.Bonus = 10;
			eldritch_staff_template.DamagePerSecond = 30;
			eldritch_staff_template.Speed = 4400;
			eldritch_staff_template.Weight = 45;
			eldritch_staff_template.Model = 19;
			eldritch_staff_template.Realm = eRealm.Hibernia;
			eldritch_staff_template.IsDropable = true; 
			eldritch_staff_template.IsTradable = false; 
			eldritch_staff_template.IsSaleable = false;
			eldritch_staff_template.MaterialLevel = eMaterialLevel.Bronze;
			
			eldritch_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Void, 4));
			
			if(!allStartupItems.Contains("Eldritch_Staff_of_Focus"))
			{
				allStartupItems.Add("Eldritch_Staff_of_Focus", eldritch_staff_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + eldritch_staff_template.Name + " to EldritchTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Eldritch"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Eldritch)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"Drink up this knowledge, " + player.Name + ", and remember it, for there shall be a day when I no longer rise in the morning, and you may be required to take my place.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Greetings, " + player.Name + ". It is my understanding that you have chosen the Path of Focus, and wish to train as an [Eldritch].\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Magician && (player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen));
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
				case "Eldritch":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Eldritch, "I can give you the gift of knowledge, but wisdom you must seek on your own. I welcome you, " + source.GetName(0, false) + ". Here, take this welcoming gift. Use it wisely.", new GenericItemTemplate[] {allStartupItems["Eldritch_Staff_of_Focus"] as GenericItemTemplate});
				
				break;
			}
			return true;		
		}
	}
}
