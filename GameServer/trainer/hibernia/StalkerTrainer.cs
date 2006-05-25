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
	/// Stalker Trainer
	/// </summary>	
	public class StalkerTrainer : GameStandardTrainer
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
			#region Training dirk

			PiercingTemplate training_dirk_template = new PiercingTemplate();
			training_dirk_template.Name = "training dirk";
			training_dirk_template.Level = 0;
			training_dirk_template.Durability = 100;
			training_dirk_template.Condition = 100;
			training_dirk_template.Quality = 90;
			training_dirk_template.Bonus = 0;
			training_dirk_template.DamagePerSecond = 12;
			training_dirk_template.Speed = 2200;
			training_dirk_template.HandNeeded = eHandNeeded.LeftHand;
			training_dirk_template.Weight = 9;
			training_dirk_template.Model = 454;
			training_dirk_template.Realm = eRealm.Hibernia;
			training_dirk_template.IsDropable = true; 
			training_dirk_template.IsTradable = false; 
			training_dirk_template.IsSaleable = false;
			training_dirk_template.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains(training_dirk_template))
			{
				allStartupItems.Add(training_dirk_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_dirk_template.Name + " to StalkerTrainer gifts.");
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
			get { return "Stalker"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Stalker; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;

			player.Out.SendMessage(this.Name + " says, \"[Ranger] or [Nightshades]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

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
			case "Ranger":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of an Ranger is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Nightshade":
				if(player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Nightshade is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
