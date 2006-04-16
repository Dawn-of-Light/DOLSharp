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
using DOL.GS.Database;
using log4net;

namespace DOL.GS.Trainer
{
	/// <summary>
	/// Magician Trainer
	/// </summary>	
	public class MagicianTrainer : GameStandardTrainer
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
			#region Training staff

			StaffTemplate training_staff_hib_template = new StaffTemplate();
			training_staff_hib_template.Name = "training staff";
			training_staff_hib_template.Level = 0;
			training_staff_hib_template.Durability = 100;
			training_staff_hib_template.Condition = 100;
			training_staff_hib_template.Quality = 90;
			training_staff_hib_template.Bonus = 0;
			training_staff_hib_template.DamagePerSecond = 12;
			training_staff_hib_template.Speed = 4500;
			training_staff_hib_template.Weight = 45;
			training_staff_hib_template.Model = 19;
			training_staff_hib_template.Realm = eRealm.Hibernia;
			training_staff_hib_template.IsDropable = true; 
			training_staff_hib_template.IsTradable = false; 
			training_staff_hib_template.IsSaleable = false;
			training_staff_hib_template.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains(training_staff_hib_template))
			{
				allStartupItems.Add(training_staff_hib_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_staff_hib_template.Name + " to MagicianTrainer gifts.");
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
			get { return "Magician"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Magician; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;

			player.Out.SendMessage(this.Name + " says, \"[Eldritch], [Enchanter] or [Mentalist]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

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
			case "Eldritch":
				if(player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Eldritch is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Enchanter":
				if(player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen){
					player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Enchanter is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Mentalist":
				if(player.Race == (int) eRace.Celt || player.Race == (int) eRace.Elf || player.Race == (int) eRace.Lurikeen || player.Race == (int) eRace.Shar)
				{
				player.Out.SendMessage(this.Name + " says, \"I can't tell you something about this class.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				else
				{
					player.Out.SendMessage(this.Name + " says, \"The path of a Mentalist is not available to your race. Please choose another.\"", eChatType.CT_System, eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
