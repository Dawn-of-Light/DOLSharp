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
	/// Friar Trainer
	/// </summary>	
	[NPCGuildScript("Friar Trainer", eRealm.Albion)]		// this attribute instructs DOL to use this script for all "Friar Trainer" NPC's in Albion (multiple guilds are possible for one script)
	public class FriarTrainer : GameTrainer
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
			#region Friar torse armor

			TorsoArmorTemplate friar_template = new TorsoArmorTemplate();
			friar_template.Name = "Robes of the Novice";
			friar_template.Level = 5;
			friar_template.Durability=100;
			friar_template.Condition = 100;
			friar_template.Quality = 100;
			friar_template.Bonus = 10;	
			friar_template.ArmorFactor = 14;
			friar_template.ArmorLevel = eArmorLevel.Low;								
			friar_template.Weight = 15;
			friar_template.Model = 58;
			friar_template.Realm = eRealm.Albion;
			friar_template.IsDropable = true; 
			friar_template.IsTradable = false; 
			friar_template.IsSaleable = false;
			friar_template.MaterialLevel = eMaterialLevel.Bronze;
			
			friar_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Dexterity, 1));
			friar_template.MagicalBonus.Add(new ItemMagicalBonus(eProperty.Constitution, 1));
				
			if(!allStartupItems.Contains("Robes_of_the_Novice"))
			{
				allStartupItems.Add("Robes_of_the_Novice", friar_template);
	
				if (log.IsDebugEnabled)
					log.Debug("Adding " + friar_template.Name + " to FriarTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Friar"; }
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
			if (player.CharacterClass.ID == (int) eCharacterClass.Friar) {

				// popup the training window
				player.Out.SendTrainerWindow();
				//player.Out.SendMessage(this.Name + " says, \"Select what you like to train.\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			} else {
				// perhaps player can be promoted
				if (CanPromotePlayer(player)) {
					player.Out.SendMessage(this.Name + " says, \"Do you desire to [join the Defenders of Albion] and defend our realm as a Friar?\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				} else {
					player.Out.SendMessage(this.Name + " says, \"You must seek elsewhere for your training.\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);							
				}
			}
			return true;
 		}

		/// <summary>
		/// checks wether a player can be promoted or not
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanPromotePlayer(GamePlayer player)
		{
			return (player.Level>=5 && player.CharacterClass.ID == (int) eCharacterClass.Acolyte && player.Race == (int) eRace.Briton);
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
	
			switch (text) {
			case "join the Defenders of Albion":
				// promote player to other class
				if (CanPromotePlayer(player)) 
					PromotePlayer(player, (int)eCharacterClass.Friar, "We welcome you into our society as an equal! We are at your disposal. We will now issue your Robes of the Novice. Wear them always and let it serve to remind you of your faith. When you have reached the title of Lesser Chaplain, return them to me. We shall then see if you require another.", new GenericItemTemplate[] {allStartupItems["Robes_of_the_Novice"] as GenericItemTemplate});
					
				break;
			}
			return true;		
		}
	}
}
