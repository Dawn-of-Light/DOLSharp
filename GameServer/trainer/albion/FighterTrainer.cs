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
	/// Fighter Trainer
	/// </summary>	
	public class FighterTrainer : GameStandardTrainer
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
			#region Practice sword

			SlashingWeaponTemplate practice_sword_template = new SlashingWeaponTemplate();
			practice_sword_template.Name = "practice sword";
			practice_sword_template.Level = 0;
			practice_sword_template.Durability = 100;
			practice_sword_template.Condition = 100;
			practice_sword_template.Quality = 90;
			practice_sword_template.Bonus = 0;
			practice_sword_template.DamagePerSecond = 12;
			practice_sword_template.Speed = 2500;
			practice_sword_template.Weight = 10;
			practice_sword_template.Model = 3;
			practice_sword_template.Realm = eRealm.Albion;
			practice_sword_template.IsDropable = true; 
			practice_sword_template.IsTradable = false; 
			practice_sword_template.IsSaleable = false;
			practice_sword_template.MaterialLevel = eMaterialLevel.Bronze;
	
			if(!allStartupItems.Contains(practice_sword_template))
			{
				allStartupItems.Add(practice_sword_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + practice_sword_template.Name + " to FighterTrainer gifts.");
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
			small_training_shield_template.Realm = eRealm.Albion;
			small_training_shield_template.IsDropable = true; 
			small_training_shield_template.IsTradable = false; 
			small_training_shield_template.IsSaleable = false;
			small_training_shield_template.MaterialLevel = eMaterialLevel.Bronze;
		
			if(!allStartupItems.Contains(small_training_shield_template))
			{
				allStartupItems.Add(small_training_shield_template);
			
				if (log.IsDebugEnabled)
					log.Debug("Adding " + small_training_shield_template.Name + " to FighterTrainer gifts.");
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
			get { return "Fighter"; }
		}

		/// <summary>
		/// Gets trained class
		/// </summary>
		public override eCharacterClass TrainedClass
		{
			get { return eCharacterClass.Fighter; }
		}

		/// <summary>
		/// Interact with trainer
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
 		public override bool Interact(GamePlayer player)
 		{		
 			if (!base.Interact(player)) return false;
					
			player.Out.SendMessage(this.Name + " says, \"[Armsman], [Paladin], or [Mercenary]?\"", eChatType.CT_Say, eChatLoc.CL_PopupWindow);												

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
			case "Armsman":
				if(player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.Briton || player.Race == (int) eRace.HalfOgre || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"Ah! An Armsmen is it? Good solid fighters they are! Their fighting prowess is a great asset to Albion. To become an armsman you must enlist with the Defenders of Albion.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of an Armsman is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Mercenary":
				if(player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.Briton || player.Race == (int) eRace.HalfOgre || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Inconnu || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"You wish to become a Mercenary do you? Roguish fighters in nature, solid warriors in battle, their ability to quickly evade enemy attacks has made them a valuable asset to the Guild of Shadows.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Mercenary is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			case "Paladin":
				if(player.Race == (int) eRace.Avalonian || player.Race == (int) eRace.Briton || player.Race == (int) eRace.Highlander || player.Race == (int) eRace.Saracen){
					player.Out.SendMessage(this.Name + " says, \"You wish to be a defender of the faith I take it? Many a Paladin has led our fighters into battle with victory not far behind. Their never-ending sacrifice proves that the Church of Albion will remain for many centuries!\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				else{
					player.Out.SendMessage(this.Name + " says, \"The path of a Paladin is not available to your race. Please choose another.\"",eChatType.CT_Say,eChatLoc.CL_PopupWindow);
				}
				return true;
			}
			return true;			
		}
	}
}
