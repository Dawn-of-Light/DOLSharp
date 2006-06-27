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
	/// Druid Trainer
	/// </summary>	
	[NPCGuildScript("Druid Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Druid Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class DruidTrainer : GameTrainer
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
			#region Driud gloves

			HandsArmorTemplate druid_gloves_template = new HandsArmorTemplate();
			druid_gloves_template.Name = "Druid Initiate Gloves";
			druid_gloves_template.Level = 5;
			druid_gloves_template.Durability = 100;
			druid_gloves_template.Condition = 100;
			druid_gloves_template.Quality = 90;
			druid_gloves_template.Bonus = 10;
			druid_gloves_template.ArmorLevel = eArmorLevel.Low;
			druid_gloves_template.ArmorFactor = 14;
			druid_gloves_template.Weight = 16;
			druid_gloves_template.Model = 356;
			druid_gloves_template.Realm = eRealm.Hibernia;
			druid_gloves_template.IsDropable = true; 
			druid_gloves_template.IsTradable = false; 
			druid_gloves_template.IsSaleable = false;
			druid_gloves_template.MaterialLevel = eMaterialLevel.Bronze;
			
			druid_gloves_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Empathy, 1));
			
			if(!allStartupItems.Contains("Druid_Initiate_Gloves"))
			{
				allStartupItems.Add("Druid_Initiate_Gloves", druid_gloves_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + druid_gloves_template.Name + " to DruidTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Druid)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"I shall impart all that I know, young Druid.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Do you wish to walk the Path of Harmony and learn the ways of the [Druid]?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
				case "Druid":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Druid, "The path of the Druid suits you, " + source.GetName(0, false) + ". Welcome. Take this, " + source.GetName(0, false) + ". You are a Druid now. Stick to our ways, and you shall go far.", new GenericItemTemplate[] {allStartupItems["Druid_Initiate_Gloves"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
