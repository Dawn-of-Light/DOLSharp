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
	/// Bard Trainer
	/// </summary>	
	[NPCGuildScript("Bard Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Bard Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class BardTrainer : GameTrainer
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
			#region Bard Lute

			Instrument bard_lute_template = new Instrument();
			bard_lute_template.Name = "Bard Initiate Lute";
			bard_lute_template.Level = 5;
			bard_lute_template.Durability = 100;
			bard_lute_template.Condition = 100;
			bard_lute_template.Quality = 90;
			bard_lute_template.Bonus = 10;
			bard_lute_template.Type = eInstrumentType.Lute;
			bard_lute_template.Weight = 45;
			bard_lute_template.Model = 227;
			bard_lute_template.Realm = eRealm.Hibernia;
			bard_lute_template.IsDropable = true; 
			bard_lute_template.IsTradable = false; 
			bard_lute_template.IsSaleable = false;
			bard_lute_template.MaterialLevel = eMaterialLevel.Bronze;
			
			bard_lute_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Instruments, 1));
			
			if(!allStartupItems.Contains("Bard_Initiate_Lute"))
			{
				allStartupItems.Add("Bard_Initiate_Lute", bard_lute_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + bard_lute_template.Name + " to BardTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Bard"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Bard)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"Ahh, well met " + player.Name + ". Back for more lore and knowledge, eh.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"Do you wish to walk the Path of Essence, pursuing a life of storytelling and [song]? Of brave deeds which grow braver with every telling?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Naturalist && (player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg));
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
				case "song":
					if (CanPromotePlayer(player)) 
						PromotePlayer(player, (int)eCharacterClass.Bard, "Welcome then, " + source.GetName(0, false) + ", to the Bard's life. Here, take this. Keep it well, " + source.GetName(0, false) + ", for the tools of our trade can be quite expensive.", new GenericItemTemplate[] {allStartupItems["Bard_Initiate_Lute"] as GenericItemTemplate});					
					
				break;
			}
			return true;		
		}
	}
}
