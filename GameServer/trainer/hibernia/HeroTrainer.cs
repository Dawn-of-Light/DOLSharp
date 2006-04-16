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
	/// Hero Trainer
	/// </summary>	
	[NPCGuildScript("Hero Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Hero Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class HeroTrainer : GameTrainer
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
			#region Hero Gauntlets

			HandsArmorTemplate hero_gloves_template = new HandsArmorTemplate();
			hero_gloves_template.Name = "Hero Initiate Gauntlets";
			hero_gloves_template.Level = 5;
			hero_gloves_template.Durability = 100;
			hero_gloves_template.Condition = 100;
			hero_gloves_template.Quality = 90;
			hero_gloves_template.Bonus = 10;
			hero_gloves_template.ArmorLevel = eArmorLevel.Medium;
			hero_gloves_template.ArmorFactor = 14;
			hero_gloves_template.Weight = 24;
			hero_gloves_template.Model = 346;
			hero_gloves_template.Realm = eRealm.Hibernia;
			hero_gloves_template.IsDropable = true; 
			hero_gloves_template.IsTradable = false; 
			hero_gloves_template.IsSaleable = false;
			hero_gloves_template.MaterialLevel = eMaterialLevel.Bronze;
			
			hero_gloves_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Strength, 1));
			
			if(!allStartupItems.Contains("Hero_Initiate_Gauntlets"))
			{
				allStartupItems.Add("Hero_Initiate_Gauntlets", hero_gloves_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + hero_gloves_template.Name + " to HeroTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Hero"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Hero)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"Training makes for a strong, healthy Hero! Keep up the good work, " + player.Name + "!\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"You wish for the life of a [hero]? You wish to walk the Path of Focus?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Guardian && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg
				|| player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar || player.Race == (int) eRace.Sylvan));
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
				case "hero":
					if (CanPromotePlayer(player))
						PromotePlayer(player, (int)eCharacterClass.Hero, "Let me welcome you then, " + source.GetName(0, false) + ". Go, honor us with your deeds. Here is a gift for you. Try not to drop it, eh " + source.GetName(0, false) + ".", new GenericItemTemplate[] {allStartupItems["Hero_Initiate_Gauntlets"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
