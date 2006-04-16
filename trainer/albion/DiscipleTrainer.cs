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
	/// Disciple Trainer
	/// </summary>	
	public class DiscipleTrainer : GameStandardTrainer
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
			#region Trimmed branch

			StaffTemplate trimmed_branch_template = new StaffTemplate();
			trimmed_branch_template.Name = "trimmed branch";
			trimmed_branch_template.Level = 0;
			trimmed_branch_template.Durability = 100;
			trimmed_branch_template.Condition = 100;
			trimmed_branch_template.Quality = 90;
			trimmed_branch_template.Bonus = 0;
			trimmed_branch_template.DamagePerSecond = 12;
			trimmed_branch_template.Speed = 2700;
			trimmed_branch_template.Weight = 12;
			trimmed_branch_template.Model = 19;
			trimmed_branch_template.Realm = eRealm.Albion;
			trimmed_branch_template.IsDropable = true; 
			trimmed_branch_template.IsTradable = false; 
			trimmed_branch_template.IsSaleable = false;
			trimmed_branch_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains(trimmed_branch_template))
			{
				allStartupItems.Add(trimmed_branch_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + trimmed_branch_template.Name + " to DiscipleTrainer gifts.");
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
			get { return "Disciple"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Disciple; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;

			player.Out.SendMessage(this.Name + " says, \"[Necromancer]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												
			
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
			case "Necromancer":
				if(player.Race == (int) eRace.Briton || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"So you want to become a Necromancer? As a Necromancer you can summon undeath creatures.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Necromancer is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			
			}
			return true;			
		}
	}
}
