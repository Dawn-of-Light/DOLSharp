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
using DOL.GS.PacketHandler;
using DOL.GS.Database;
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Seer Trainer
	/// </summary>	
	public class SeerTrainer : GameStandardTrainer
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This function is called at the server startup
		/// </summary>	
		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			#region Training hammer

			HammerTemplate training_hammer_template = new HammerTemplate();
			training_hammer_template.Name = "training hammer";
			training_hammer_template.Level = 0;
			training_hammer_template.Durability = 100;
			training_hammer_template.Condition = 100;
			training_hammer_template.Quality = 90;
			training_hammer_template.Bonus = 0;
			training_hammer_template.DamagePerSecond = 13;
			training_hammer_template.Speed = 3000;
			training_hammer_template.HandNeeded = eHandNeeded.RightHand;
			training_hammer_template.Weight = 32;
			training_hammer_template.Model = 321;
			training_hammer_template.Realm = eRealm.Midgard;
			training_hammer_template.IsDropable = true; 
			training_hammer_template.IsTradable = false; 
			training_hammer_template.IsSaleable = false;
			training_hammer_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains(training_hammer_template))
			{
				allStartupItems.Add(training_hammer_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_hammer_template.Name + " to SeerTrainer gifts.");
			}
			#endregion

			#region Training shield

			ShieldTemplate small_training_shield_template = new ShieldTemplate();
			small_training_shield_template.Name = "small training shield";
			small_training_shield_template.Level = 2;
			small_training_shield_template.Durability = 100;
			small_training_shield_template.Condition = 100;
			small_training_shield_template.Quality = 100;
			small_training_shield_template.Bonus = 0;
			small_training_shield_template.DamagePerSecond = 10;
			small_training_shield_template.Speed = 2000;
			small_training_shield_template.Size = eShieldSize.Small;
			small_training_shield_template.Weight = 32;
			small_training_shield_template.Model = 59;
			small_training_shield_template.Realm = eRealm.Midgard;
			small_training_shield_template.IsDropable = true; 
			small_training_shield_template.IsTradable = false; 
			small_training_shield_template.IsSaleable = false;
			small_training_shield_template.MaterialLevel = eMaterialLevel.Bronze;
		
			if(!allStartupItems.Contains(small_training_shield_template))
			{
				allStartupItems.Add(small_training_shield_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + small_training_shield_template.Name + " to SeerTrainer gifts.");
			}
			#endregion
		}

		/// <summary>
		/// This hash constrain all item template the trainer can give
		/// </summary>	
		protected static IList allStartupItems = new ArrayList();

		/// <summary>
		/// Gets all trainer gifts
		/// </summary>
		public override IList TrainerGifts
		{
			get { return allStartupItems; }
		}

		/// <summary>
		/// Gets trainer classname
		/// </summary>
		public override string TrainerClassName
		{
			get { return "Seer"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Seer; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;

			player.Out.SendMessage(this.Name + " says, \"[Shaman] or [Healer]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

			return true;
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
			case "Shaman":
				if(player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Kobold || player.Race == (int) eRace.Troll){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Shaman is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Healer":
				if(player.Race == (int) eRace.Dwarf || player.Race == (int) eRace.Frostalf || player.Race == (int) eRace.Norseman){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Healer is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
