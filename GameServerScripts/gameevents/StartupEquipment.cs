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
using log4net;

namespace DOL.GS.GameEvents
{
	/// <summary>
	/// Equips new created Characters with standard equipment
	/// </summary>
	public class StartupEquipment
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			//We want to be notified whenever a new character is created
			GameEventMgr.AddHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));

			#region Alb startup equipment template

			SlashingWeaponTemplate practice_sword_template = new SlashingWeaponTemplate();
			practice_sword_template.Name = "practice sword";
			practice_sword_template.Level = 1;
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

			if(!allStartupItems.Contains("practice_sword"))
			{
				allStartupItems.Add("practice_sword", practice_sword_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + practice_sword_template.Name + " to the startup equipment.");
			}

			
			ThrustWeaponTemplate practice_dirk_template = new ThrustWeaponTemplate();
			practice_dirk_template.Name = "practice dirk";
			practice_dirk_template.Level = 1;
			practice_dirk_template.Durability = 100;
			practice_dirk_template.Condition = 100;
			practice_dirk_template.Quality = 90;
			practice_dirk_template.Bonus = 0;
			practice_dirk_template.DamagePerSecond = 12;
			practice_dirk_template.Speed = 2200;
			practice_dirk_template.Weight = 8;
			practice_dirk_template.Model = 21;
			practice_dirk_template.Realm = eRealm.Albion;
			practice_dirk_template.IsDropable = true; 
			practice_dirk_template.IsTradable = false; 
			practice_dirk_template.IsSaleable = false;
			practice_dirk_template.MaterialLevel = eMaterialLevel.Bronze;

			if(!allStartupItems.Contains("practice_dirk"))
			{
				allStartupItems.Add("practice_dirk", practice_dirk_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + practice_dirk_template.Name + " to the startup equipment.");
			}

			CrushingWeaponTemplate training_mace_template = new CrushingWeaponTemplate();
			training_mace_template.Name = "practice mace";
			training_mace_template.Level = 1;
			training_mace_template.Durability = 100;
			training_mace_template.Condition = 100;
			training_mace_template.Quality = 90;
			training_mace_template.Bonus = 0;
			training_mace_template.DamagePerSecond = 12;
			training_mace_template.Speed = 2700;
			training_mace_template.Weight = 30;
			training_mace_template.Model = 13;
			training_mace_template.Realm = eRealm.Albion;
			training_mace_template.IsDropable = true; 
			training_mace_template.IsTradable = false; 
			training_mace_template.IsSaleable = false;
			training_mace_template.MaterialLevel = eMaterialLevel.Bronze;
				
			if(!allStartupItems.Contains("practice_mace"))
			{
				allStartupItems.Add("practice_mace", training_mace_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_mace_template.Name + " to the startup equipment.");
			}
			
			StaffTemplate trimmed_branch_template = new StaffTemplate();
			trimmed_branch_template.Name = "trimmed branch";
			trimmed_branch_template.Level = 1;
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
			
			if(!allStartupItems.Contains("trimmed_branch"))
			{
				allStartupItems.Add("trimmed_branch", trimmed_branch_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + trimmed_branch_template.Name + " to the startup equipment.");
			}

			
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
		
			if(!allStartupItems.Contains("small_training_shield_alb"))
			{
				allStartupItems.Add("small_training_shield_alb", small_training_shield_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + small_training_shield_template.Name + " to the startup equipment.");
			}
			#endregion

			#region Mid startup equipment template
			
			AxeTemplate training_axe_template = new AxeTemplate();
			training_axe_template.Name = "training axe";
			training_axe_template.Level = 1;
			training_axe_template.Durability = 100;
			training_axe_template.Condition = 100;
			training_axe_template.Quality = 90;
			training_axe_template.Bonus = 0;
			training_axe_template.DamagePerSecond = 13;
			training_axe_template.Speed = 2500;
			training_axe_template.HandNeeded = eHandNeeded.LeftHand;
			training_axe_template.Weight = 20;
			training_axe_template.Model = 316;
			training_axe_template.Realm = eRealm.Midgard;
			training_axe_template.IsDropable = true; 
			training_axe_template.IsTradable = false; 
			training_axe_template.IsSaleable = false;
			training_axe_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains("training_axe"))
			{
				allStartupItems.Add("training_axe", training_axe_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_axe_template.Name + " to the startup equipment.");
			}

			
			SwordTemplate training_sword_mid_template = new SwordTemplate();
			training_sword_mid_template.Name = "training sword";
			training_sword_mid_template.Level = 1;
			training_sword_mid_template.Durability = 100;
			training_sword_mid_template.Condition = 100;
			training_sword_mid_template.Quality = 90;
			training_sword_mid_template.Bonus = 0;
			training_sword_mid_template.DamagePerSecond = 13;
			training_sword_mid_template.Speed = 2500;
			training_sword_mid_template.HandNeeded = eHandNeeded.RightHand;
			training_sword_mid_template.Weight = 20;
			training_sword_mid_template.Model = 311;
			training_sword_mid_template.Realm = eRealm.Midgard;
			training_sword_mid_template.IsDropable = true; 
			training_sword_mid_template.IsTradable = false; 
			training_sword_mid_template.IsSaleable = false;
			training_sword_mid_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains("training_sword"))
			{
				allStartupItems.Add("training_sword", training_sword_mid_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_sword_mid_template.Name + " to the startup equipment.");
			}

			HammerTemplate training_hammer_template = new HammerTemplate();
			training_hammer_template.Name = "training hammer";
			training_hammer_template.Level = 1;
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
			
			if(!allStartupItems.Contains("training_hammer"))
			{
				allStartupItems.Add("training_hammer", training_hammer_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_hammer_template.Name + " to the startup equipment.");
			}


			StaffTemplate training_staff_template = new StaffTemplate();
			training_staff_template.Name = "training staff";
			training_staff_template.Level = 1;
			training_staff_template.Durability = 100;
			training_staff_template.Condition = 100;
			training_staff_template.Quality = 90;
			training_staff_template.Bonus = 0;
			training_staff_template.DamagePerSecond = 12;
			training_staff_template.Speed = 4500;
			training_staff_template.Weight = 45;
			training_staff_template.Model = 19;
			training_staff_template.Realm = eRealm.Midgard;
			training_staff_template.IsDropable = true; 
			training_staff_template.IsTradable = false; 
			training_staff_template.IsSaleable = false;
			training_staff_template.MaterialLevel = eMaterialLevel.Bronze;
				
			if(!allStartupItems.Contains("training_staff"))
			{
				allStartupItems.Add("training_staff", training_staff_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_staff_template.Name + " to the startup equipment.");
			}

			
			ShieldTemplate small_training_shield_template_mid = new ShieldTemplate();
			small_training_shield_template_mid.Name = "small training shield";
			small_training_shield_template_mid.Level = 2;
			small_training_shield_template_mid.Durability = 100;
			small_training_shield_template_mid.Condition = 100;
			small_training_shield_template_mid.Quality = 100;
			small_training_shield_template_mid.Bonus = 0;
			small_training_shield_template_mid.DamagePerSecond = 10;
			small_training_shield_template_mid.Speed = 2000;
			small_training_shield_template_mid.Size = eShieldSize.Small;
			small_training_shield_template_mid.Weight = 32;
			small_training_shield_template_mid.Model = 59;
			small_training_shield_template_mid.Realm = eRealm.Midgard;
			small_training_shield_template_mid.IsDropable = true; 
			small_training_shield_template_mid.IsTradable = false; 
			small_training_shield_template_mid.IsSaleable = false;
			small_training_shield_template_mid.MaterialLevel = eMaterialLevel.Bronze;
				
			if(!allStartupItems.Contains("small_training_shield_mid"))
			{
				allStartupItems.Add("small_training_shield_mid", small_training_shield_template_mid);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + small_training_shield_template_mid.Name + " to the startup equipment.");
			}

			#endregion

			#region Hib startup equipment template

			BladesTemplate training_sword_hib_template = new BladesTemplate();
			training_sword_hib_template.Name = "training sword";
			training_sword_hib_template.Level = 1;
			training_sword_hib_template.Durability = 100;
			training_sword_hib_template.Condition = 100;
			training_sword_hib_template.Quality = 90;
			training_sword_hib_template.Bonus = 0;
			training_sword_hib_template.DamagePerSecond = 12;
			training_sword_hib_template.Speed = 2500;
			training_sword_hib_template.HandNeeded = eHandNeeded.LeftHand;
			training_sword_hib_template.Weight = 18;
			training_sword_hib_template.Model = 445;
			training_sword_hib_template.Realm = eRealm.Hibernia;
			training_sword_hib_template.IsDropable = true; 
			training_sword_hib_template.IsTradable = false; 
			training_sword_hib_template.IsSaleable = false;
			training_sword_hib_template.MaterialLevel = eMaterialLevel.Bronze;
			
			if(!allStartupItems.Contains("training_sword_hib"))
			{
				allStartupItems.Add("training_sword_hib", training_sword_hib_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_sword_hib_template.Name + " to the startup equipment.");
			}

			
			PiercingTemplate training_dirk_template = new PiercingTemplate();
			training_dirk_template.Name = "training dirk";
			training_dirk_template.Level = 1;
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
			
			if(!allStartupItems.Contains("training_dirk_hib"))
			{
				allStartupItems.Add("training_dirk_hib", training_dirk_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_dirk_template.Name + " to the startup equipment.");
			}

			
			BluntTemplate training_club_template_hib = new BluntTemplate();
			training_club_template_hib.Name = "training club";
			training_club_template_hib.Level = 1;
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
			
			if(!allStartupItems.Contains("training_club_hib"))
			{
				allStartupItems.Add("training_club_hib", training_club_template_hib);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_club_template_hib.Name + " to the startup equipment.");
			}
			
			StaffTemplate training_staff_hib_template = new StaffTemplate();
			training_staff_hib_template.Name = "training staff";
			training_staff_hib_template.Level = 1;
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
				
			if(!allStartupItems.Contains("training_staff_hib"))
			{
				allStartupItems.Add("training_staff_hib", training_staff_hib_template);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_staff_hib_template.Name + " to the startup equipment.");
			}

			
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
			
			if(!allStartupItems.Contains("training_shield_hib"))
			{
				allStartupItems.Add("training_shield_hib", training_shield_template_hib);
				
				if (log.IsDebugEnabled)
					log.Debug("Adding " + training_staff_hib_template.Name + " to the startup equipment.");
			}
			#endregion			
		}

		[ScriptUnloadedEvent]
		public static void OnScriptUnloaded(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.RemoveHandler(DatabaseEvent.CharacterCreated, new DOLEventHandler(CharacterCreation));
		}

		/// <summary>
		/// The collection where all statutup item template are stored
		/// </summary>
		private static IDictionary allStartupItems = new Hashtable();

		
		/// <summary>
		/// This function is called each time a character is created
		/// </summary>
		public static void CharacterCreation(DOLEvent e, object sender, EventArgs args)
		{
			CharacterEventArgs charArgs = args as CharacterEventArgs;
			if (charArgs == null)
				return;

			ArrayList itemToAdd = new ArrayList();
			switch (charArgs.Character.CharacterClassID)
			{
	//Alb Classes
				case 14:
					itemToAdd.Add("practice_sword");
					itemToAdd.Add("small_training_shield_alb");
					break; //Fighter
				case 15:
					itemToAdd.Add("trimmed_branch");
					break; //Elementalist
				case 16:
					itemToAdd.Add("practice_mace");
					itemToAdd.Add("small_training_shield_alb");
					break; //Acolyte
				case 17:
					itemToAdd.Add("practice_dirk");
					break; //Alb Rouge
				case 18:
					itemToAdd.Add("trimmed_branch");
					break; //Mage
				case 20:
					itemToAdd.Add("trimmed_branch");
					break; //Disciple
	//Mid Classes
				case 35:
					itemToAdd.Add("training_axe");
					itemToAdd.Add("small_training_shield");
					break; //Viking
				case 36:
					itemToAdd.Add("training_staff");
					break; //Mystic
				case 37:
					itemToAdd.Add("training_hammer");
					itemToAdd.Add("small_training_shield_mid");
					break; //Seer
				case 38:
					itemToAdd.Add("training_sword");
					break; //Mid Rogue
	//Hib Classes
				case 51:
					itemToAdd.Add("training_staff_hib");
					break; //Magician
				case 52:
					itemToAdd.Add("training_sword_hib");
					itemToAdd.Add("training_shield_hib");
					break; //Guardian
				case 53:
					itemToAdd.Add("training_club_hib");
					itemToAdd.Add("training_shield_hib");
					break; //Naturalist
				case 54:
					itemToAdd.Add("training_dirk_hib");
					break; //Stalker
				case 57:
					itemToAdd.Add("training_staff_hib");
					break; //Forester?
				default:
					if (log.IsWarnEnabled)
						log.Warn("No standard equipment defined for Character Class " + charArgs.Character.CharacterClassID + "!");
					return;
			}

			// Add items to inventory
			foreach (string templateName in itemToAdd)
			{
				GenericItemTemplate template = allStartupItems[templateName] as GenericItemTemplate;
				if(template != null)
				{
					charArgs.Character.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack , template.CreateInstance());
				}
			}
		}
	}
}