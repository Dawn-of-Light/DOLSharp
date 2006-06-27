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
	/// Valewalker Trainer
	/// </summary>	
	[NPCGuildScript("Valewalker Trainer", eRealm.Hibernia)]		// this attribute instructs DOL to use this script for all "Valewalker Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class ValewalkerTrainer : GameTrainer
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
			#region Valwalker scythe

			ScytheTemplate valwalker_scythe_item_template = new ScytheTemplate();
			valwalker_scythe_item_template.Name = "Scythe of the Initiate";
			valwalker_scythe_item_template.Level = 5;
			valwalker_scythe_item_template.Durability = 100;
			valwalker_scythe_item_template.Condition = 100;
			valwalker_scythe_item_template.Quality = 90;
			valwalker_scythe_item_template.Bonus = 10;
			valwalker_scythe_item_template.DamagePerSecond = 26;
			valwalker_scythe_item_template.Speed = 4200;
			valwalker_scythe_item_template.Weight = 35;
			valwalker_scythe_item_template.Model = 929;
			valwalker_scythe_item_template.Realm = eRealm.Hibernia;
			valwalker_scythe_item_template.IsDropable = true; 
			valwalker_scythe_item_template.IsTradable = false; 
			valwalker_scythe_item_template.IsSaleable = false;
			valwalker_scythe_item_template.MaterialLevel = eMaterialLevel.Bronze;
			
			valwalker_scythe_item_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Skill_Scythe, 1));
			
			if(!allStartupItems.Contains("Scythe_of_the_Initiate"))
			{
				allStartupItems.Add("Scythe_of_the_Initiate", valwalker_scythe_item_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + valwalker_scythe_item_template.Name + " to ValewalkerTrainer gifts.");
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Valewalker)
			{
				player.Out.SendTrainerWindow();
				player.Out.SendMessage(this.Name + " says, \"Training makes for a strong, healthy Hero! Keep up the good work, " + player.Name + "!\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			} 
			else if (CanPromotePlayer(player)) 
			{
				player.Out.SendMessage(this.Name + " says, \"You wish to follow the [Path of Affinity] and walk as a Valewalker?\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
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
						PromotePlayer(player, (int)eCharacterClass.Valewalker, "Welcome, then, to the Path of Affinity. Here is a gift. Consider it a welcoming gesture. Welcome, " + source.GetName(0, false) + ".", new GenericItemTemplate[] {allStartupItems["Scythe_of_the_Initiate"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
