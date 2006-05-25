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
	/// Naturalist Trainer
	/// </summary>	
	public class NaturalistTrainer : GameStandardTrainer
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
			#region Training club

			BluntTemplate training_club_template_hib = new BluntTemplate();
			training_club_template_hib.Name = "training club";
			training_club_template_hib.Level = 0;
			training_club_template_hib.Durability = 100;
			training_club_template_hib.Condition = 100;
			training_club_template_hib.Quality = 90;
			training_club_template_hib.Bonus = 0;
			training_club_template_hib.DamagePerSecond = 12;
			training_club_template_hib.Speed = 4000;
			training_club_template_hib.HandNeeded = eHandNeeded.RightHand;
			training_club_template_hib.Weight = 35;
			training_club_template_hib.Model = 449;
			training_club_template_hib.Realm = eRealm.Hibernia;
			training_club_template_hib.IsDropable = true; 
			training_club_template_hib.IsTradable = false; 
			training_club_template_hib.IsSaleable = false;
			training_club_template_hib.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains(training_club_template_hib))
			{
				allStartupItems.Add(training_club_template_hib);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_club_template_hib.Name + " to NaturalistTrainer gifts.");
			}
			#endregion

			#region Training shield

			ShieldTemplate training_shield_template_hib = new ShieldTemplate();
			training_shield_template_hib.Name = "training shield";
			training_shield_template_hib.Level = 2;
			training_shield_template_hib.Durability = 100;
			training_shield_template_hib.Condition = 100;
			training_shield_template_hib.Quality = 90;
			training_shield_template_hib.Bonus = 0;
			training_shield_template_hib.DamagePerSecond = 10;
			training_shield_template_hib.Speed = 2000;
			training_shield_template_hib.Size = eShieldSize.Small;
			training_shield_template_hib.Weight = 32;
			training_shield_template_hib.Model = 59;
			training_shield_template_hib.Realm = eRealm.Hibernia;
			training_shield_template_hib.IsDropable = true; 
			training_shield_template_hib.IsTradable = false; 
			training_shield_template_hib.IsSaleable = false;
			training_shield_template_hib.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains(training_shield_template_hib))
			{
				allStartupItems.Add(training_shield_template_hib);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_shield_template_hib.Name + " to NaturalistTrainer gifts.");
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
			get { return "Naturalist"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Naturalist; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;

			player.Out.SendMessage(this.Name + " says, \"[Bard], [Druid] or [Warden]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

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
			case "Bard":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Bard is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Druid":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg || player.Race == (int) eRace.Sylvan){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Druid is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Warden":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Firbolg || player.Race == (int) eRace.Sylvan)
				{
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else
				{
					player.Out.SendMessage(this.Name + " says, \"The path of a Warden is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
