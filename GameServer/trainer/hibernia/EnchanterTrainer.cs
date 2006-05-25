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
	/// Enchanter Trainer
	/// </summary>	
	[NPCGuildScript("Enchanter Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Enchanter Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class EnchanterTrainer : GameTrainer
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
			#region Enchanter staff

			StaffTemplate enchanter_staff_template = new StaffTemplate();
			enchanter_staff_template.Name = "Enchanter Staff of Focus";
			enchanter_staff_template.Level = 5;
			enchanter_staff_template.Durability = 100;
			enchanter_staff_template.Condition = 100;
			enchanter_staff_template.Quality = 90;
			enchanter_staff_template.Bonus = 10;
			enchanter_staff_template.DamagePerSecond = 30;
			enchanter_staff_template.Speed = 4400;
			enchanter_staff_template.Weight = 45;
			enchanter_staff_template.Model = 19;
			enchanter_staff_template.Realm = eRealm.Hibernia;
			enchanter_staff_template.IsDropable = true; 
			enchanter_staff_template.IsTradable = false; 
			enchanter_staff_template.IsSaleable = false;
			enchanter_staff_template.MaterialLevel = eMaterialLevel.Bronze;
			
			enchanter_staff_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Focus_Enchantments, 4));
			
			if(!allStartupItems.Contains("Enchanter_Staff_of_Focus"))
			{
				allStartupItems.Add("Enchanter_Staff_of_Focus", enchanter_staff_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + enchanter_staff_template.Name + " to EnchanterTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Enchanter"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Enchanter)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"You are a quick study, " + player.Name + ". But do not be too hasty. There is always more to learn.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Do you choose to train as an Enchanter and follow the Path of Essence? Life as an [Enchanter] is as rewarding as it is dangerous, but it is yours for the asking.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
				case "Enchanter":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Enchanter, "Welcome, " + source.GetName(0, false) + ". From this day forth you shall be known as an Enchanter. Here, " + source.GetName(0, false) + ". Take this gift to aid you on your path.", new GenericItemTemplate[] {allStartupItems["Enchanter_Staff_of_Focus"] as GenericItemTemplate});
				
				break;
			}
			return true;		
		}
	}
}
