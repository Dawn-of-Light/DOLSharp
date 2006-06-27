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
	/// Mentalist Trainer
	/// </summary>	
	[NPCGuildScript("Mentalist Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Mentalist Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class MentalistTrainer : GameTrainer
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
			#region Mentalist Staff

			StaffTemplate mentalist_staff_template = new StaffTemplate();
			mentalist_staff_template.Name = "Staff of Mental Clarity";
			mentalist_staff_template.Level = 5;
			mentalist_staff_template.Durability = 100;
			mentalist_staff_template.Condition = 100;
			mentalist_staff_template.Quality = 90;
			mentalist_staff_template.Bonus = 10;
			mentalist_staff_template.DamagePerSecond = 30;
			mentalist_staff_template.Speed = 4400;
			mentalist_staff_template.Weight = 45;
			mentalist_staff_template.Model = 19;
			mentalist_staff_template.Realm = eRealm.Hibernia;
			mentalist_staff_template.IsDropable = true; 
			mentalist_staff_template.IsTradable = false; 
			mentalist_staff_template.IsSaleable = false;
			mentalist_staff_template.MaterialLevel = eMaterialLevel.Bronze;
			
			mentalist_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Mentalism, 4));
			
			if(!allStartupItems.Contains("Staff_of_Mental_Clarity"))
			{
				allStartupItems.Add("Staff_of_Mental_Clarity", mentalist_staff_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + mentalist_staff_template.Name + " to MentalistTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Mentalist)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"Our way is a hard one, " + player.Name + ". I can only train you in skills. You must gain knowledge and wisdom on your own.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Do you wish to train as a [Mentalist] and walk the Path of Harmony?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Magician && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf
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
				case "Mentalist":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Mentalist, "Well met then, " + source.GetName(0, false) + ". It is a hard road, but I see hardness is no stranger to you. Welcome, Mentalist, welcome. Here, take this. It will aid you in your first encounters as a Mentalist.", new GenericItemTemplate[] {allStartupItems["Staff_of_Mental_Clarity"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
