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
//created by Schaf
//(most of) this information was logged - so it should be right
using System;
using System.Reflection;
using DOL.Database;
using DOL.Events;
using log4net;

namespace DOL.GS.Items
{
	/// <summary>
	/// Creates needed items when they doesnt exist yet
	/// </summary>
	public class DefaultItems
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			ItemTemplate practice_sword_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "practice_sword");
			if (practice_sword_template == null)
			{
				practice_sword_template = new ItemTemplate();
				practice_sword_template.Name = "practice sword";
				practice_sword_template.Level = 0;
				practice_sword_template.Durability = 100;
				practice_sword_template.MaxDurability = 100;
				practice_sword_template.Condition = 50000;
				practice_sword_template.MaxCondition = 50000;
				practice_sword_template.Quality = 90;
				practice_sword_template.DPS_AF = 12;
				practice_sword_template.SPD_ABS = 25;
				practice_sword_template.Hand = 0;
				practice_sword_template.Type_Damage = 2;
				practice_sword_template.Object_Type = 3;
				practice_sword_template.Item_Type = 10;
				practice_sword_template.Weight = 10;
				practice_sword_template.Model = 3;
				practice_sword_template.IsPickable = true;
				practice_sword_template.IsDropable = false;
				practice_sword_template.Id_nb = "practice_sword";
				GameServer.Database.AddNewObject(practice_sword_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + practice_sword_template.Id_nb);
			}
			ItemTemplate practice_dirk_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "practice_dirk");
			if (practice_dirk_template == null)
			{
				practice_dirk_template = new ItemTemplate();
				practice_dirk_template.Name = "practice dirk";
				practice_dirk_template.Level = 0;
				practice_dirk_template.Durability = 100;
				practice_dirk_template.MaxDurability = 100;
				practice_dirk_template.Condition = 50000;
				practice_dirk_template.MaxCondition = 50000;
				practice_dirk_template.Quality = 90;
				practice_dirk_template.DPS_AF = 12;
				practice_dirk_template.SPD_ABS = 22;
				practice_dirk_template.Hand = 2;
				practice_dirk_template.Type_Damage = 3;
				practice_dirk_template.Object_Type = 4;
				practice_dirk_template.Item_Type = 11;
				practice_dirk_template.Weight = 8;
				practice_dirk_template.Model = 21;
				practice_dirk_template.IsPickable = true;
				practice_dirk_template.IsDropable = false;
				practice_dirk_template.Id_nb = "practice_dirk";
				GameServer.Database.AddNewObject(practice_dirk_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + practice_dirk_template.Id_nb);
			}
			ItemTemplate trimmed_branch_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "trimmed_branch");
			if (trimmed_branch_template == null)
			{
				trimmed_branch_template = new ItemTemplate();
				trimmed_branch_template.Name = "trimmed branch";
				trimmed_branch_template.Level = 0;
				trimmed_branch_template.Durability = 100;
				trimmed_branch_template.MaxDurability = 100;
				trimmed_branch_template.Condition = 50000;
				trimmed_branch_template.MaxCondition = 50000;
				trimmed_branch_template.Quality = 90;
				trimmed_branch_template.DPS_AF = 12;
				trimmed_branch_template.SPD_ABS = 27;
				trimmed_branch_template.Hand = 1;
				trimmed_branch_template.Type_Damage = 1;
				trimmed_branch_template.Object_Type = 8;
				trimmed_branch_template.Item_Type = 12;
				trimmed_branch_template.Weight = 12;
				trimmed_branch_template.Model = 19;
				trimmed_branch_template.IsPickable = true;
				trimmed_branch_template.IsDropable = false;
				trimmed_branch_template.Id_nb = "trimmed_branch";
				GameServer.Database.AddNewObject(trimmed_branch_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + trimmed_branch_template.Id_nb);
			}
			ItemTemplate training_mace_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_mace");
			if (training_mace_template == null)
			{
				training_mace_template = new ItemTemplate();
				training_mace_template.Name = "training mace";
				training_mace_template.Level = 0;
				training_mace_template.Durability = 100;
				training_mace_template.MaxDurability = 100;
				training_mace_template.Condition = 50000;
				training_mace_template.MaxCondition = 50000;
				training_mace_template.Quality = 90;
				training_mace_template.DPS_AF = 12;
				training_mace_template.SPD_ABS = 27;
				training_mace_template.Hand = 2;
				training_mace_template.Type_Damage = 1;
				training_mace_template.Object_Type = 2;
				training_mace_template.Item_Type = 11;
				training_mace_template.Weight = 30;
				training_mace_template.Model = 13;
				training_mace_template.IsPickable = true;
				training_mace_template.IsDropable = false;
				training_mace_template.Id_nb = "training_mace";
				GameServer.Database.AddNewObject(training_mace_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_mace_template.Id_nb);
			}
			ItemTemplate training_sword_hib_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_sword_hib");
			if (training_sword_hib_template == null)
			{
				training_sword_hib_template = new ItemTemplate();
				training_sword_hib_template.Name = "training sword";
				training_sword_hib_template.Level = 0;
				training_sword_hib_template.Durability = 100;
				training_sword_hib_template.MaxDurability = 100;
				training_sword_hib_template.Condition = 50000;
				training_sword_hib_template.MaxCondition = 50000;
				training_sword_hib_template.Quality = 90;
				training_sword_hib_template.DPS_AF = 12;
				training_sword_hib_template.SPD_ABS = 25;
				training_sword_hib_template.Hand = 2;
				training_sword_hib_template.Type_Damage = 2;
				training_sword_hib_template.Object_Type = 19;
				training_sword_hib_template.Item_Type = 11;
				training_sword_hib_template.Weight = 18;
				training_sword_hib_template.Model = 445;
				training_sword_hib_template.IsPickable = true;
				training_sword_hib_template.IsDropable = false;
				training_sword_hib_template.Id_nb = "training_sword_hib";
				GameServer.Database.AddNewObject(training_sword_hib_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_sword_hib_template.Id_nb);
			}
			ItemTemplate training_dirk_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_dirk");
			if (training_dirk_template == null)
			{
				training_dirk_template = new ItemTemplate();
				training_dirk_template.Name = "training dirk";
				training_dirk_template.Level = 0;
				training_dirk_template.Durability = 100;
				training_dirk_template.MaxDurability = 100;
				training_dirk_template.Condition = 50000;
				training_dirk_template.MaxCondition = 50000;
				training_dirk_template.Quality = 90;
				training_dirk_template.DPS_AF = 12;
				training_dirk_template.SPD_ABS = 22;
				training_dirk_template.Hand = 2;
				training_dirk_template.Type_Damage = 3;
				training_dirk_template.Object_Type = 21;
				training_dirk_template.Item_Type = 11;
				training_dirk_template.Weight = 9;
				training_dirk_template.Model = 454;
				training_dirk_template.IsPickable = true;
				training_dirk_template.IsDropable = false;
				training_dirk_template.Id_nb = "training_dirk";
				GameServer.Database.AddNewObject(training_dirk_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_dirk_template.Id_nb);
			}
			ItemTemplate training_club_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_club");
			if (training_club_template == null)
			{
				training_club_template = new ItemTemplate();
				training_club_template.Name = "training club";
				training_club_template.Level = 0;
				training_club_template.Durability = 100;
				training_club_template.MaxDurability = 100;
				training_club_template.Condition = 50000;
				training_club_template.MaxCondition = 50000;
				training_club_template.Quality = 90;
				training_club_template.DPS_AF = 12;
				training_club_template.SPD_ABS = 40;
				training_club_template.Hand = 0;
				training_club_template.Type_Damage = 1;
				training_club_template.Object_Type = 20;
				training_club_template.Item_Type = 10;
				training_club_template.Weight = 35;
				training_club_template.Model = 449;
				training_club_template.IsPickable = true;
				training_club_template.IsDropable = false;
				training_club_template.Id_nb = "training_club";
				GameServer.Database.AddNewObject(training_club_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_club_template.Id_nb);
			}
			ItemTemplate training_staff_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_staff");
			if (training_staff_template == null)
			{
				training_staff_template = new ItemTemplate();
				training_staff_template.Name = "training staff";
				training_staff_template.Level = 0;
				training_staff_template.Durability = 100;
				training_staff_template.MaxDurability = 100;
				training_staff_template.Condition = 50000;
				training_staff_template.MaxCondition = 50000;
				training_staff_template.Quality = 90;
				training_staff_template.DPS_AF = 12;
				training_staff_template.SPD_ABS = 45;
				training_staff_template.Hand = 1;
				training_staff_template.Type_Damage = 1;
				training_staff_template.Object_Type = 8;
				training_staff_template.Item_Type = 12;
				training_staff_template.Weight = 45;
				training_staff_template.Model = 19;
				training_staff_template.IsPickable = true;
				training_staff_template.IsDropable = false;
				training_staff_template.Id_nb = "training_staff";
				GameServer.Database.AddNewObject(training_staff_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_staff_template.Id_nb);
			}
			ItemTemplate training_axe_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_axe");
			if (training_axe_template == null)
			{
				training_axe_template = new ItemTemplate();
				training_axe_template.Name = "training axe";
				training_axe_template.Level = 0;
				training_axe_template.Durability = 100;
				training_axe_template.MaxDurability = 100;
				training_axe_template.Condition = 50000;
				training_axe_template.MaxCondition = 50000;
				training_axe_template.Quality = 90;
				training_axe_template.DPS_AF = 13;
				training_axe_template.SPD_ABS = 25;
				training_axe_template.Hand = 2;
				training_axe_template.Type_Damage = 2;
				training_axe_template.Object_Type = 13;
				training_axe_template.Item_Type = 11;
				training_axe_template.Weight = 20;
				training_axe_template.Model = 316;
				training_axe_template.IsPickable = true;
				training_axe_template.IsDropable = false;
				training_axe_template.Id_nb = "training_axe";
				GameServer.Database.AddNewObject(training_axe_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_axe_template.Id_nb);
			}
			ItemTemplate training_sword_mid_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_sword_mid");
			if (training_sword_mid_template == null)
			{
				training_sword_mid_template = new ItemTemplate();
				training_sword_mid_template.Name = "training sword";
				training_sword_mid_template.Level = 0;
				training_sword_mid_template.Durability = 100;
				training_sword_mid_template.MaxDurability = 100;
				training_sword_mid_template.Condition = 50000;
				training_sword_mid_template.MaxCondition = 50000;
				training_sword_mid_template.Quality = 90;
				training_sword_mid_template.DPS_AF = 13;
				training_sword_mid_template.SPD_ABS = 25;
				training_sword_mid_template.Hand = 0;
				training_sword_mid_template.Type_Damage = 2;
				training_sword_mid_template.Object_Type = 11;
				training_sword_mid_template.Item_Type = 10;
				training_sword_mid_template.Weight = 20;
				training_sword_mid_template.Model = 311;
				training_sword_mid_template.IsPickable = true;
				training_sword_mid_template.IsDropable = false;
				training_sword_mid_template.Id_nb = "training_sword_mid";
				GameServer.Database.AddNewObject(training_sword_mid_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_sword_mid_template.Id_nb);
			}
			ItemTemplate training_hammer_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_hammer");
			if (training_hammer_template == null)
			{
				training_hammer_template = new ItemTemplate();
				training_hammer_template.Name = "training hammer";
				training_hammer_template.Level = 0;
				training_hammer_template.Durability = 100;
				training_hammer_template.MaxDurability = 100;
				training_hammer_template.Condition = 50000;
				training_hammer_template.MaxCondition = 50000;
				training_hammer_template.Quality = 90;
				training_hammer_template.DPS_AF = 13;
				training_hammer_template.SPD_ABS = 30;
				training_hammer_template.Hand = 0;
				training_hammer_template.Type_Damage = 1;
				training_hammer_template.Object_Type = 12;
				training_hammer_template.Item_Type = 10;
				training_hammer_template.Weight = 32;
				training_hammer_template.Model = 321;
				training_hammer_template.IsPickable = true;
				training_hammer_template.IsDropable = false;
				training_hammer_template.Id_nb = "training_hammer";
				GameServer.Database.AddNewObject(training_hammer_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_hammer_template.Id_nb);
			}
			ItemTemplate small_training_shield_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "small_training_shield");
			if (small_training_shield_template == null)
			{
				small_training_shield_template = new ItemTemplate();
				small_training_shield_template.Name = "small training shield";
				small_training_shield_template.Level = 2;
				small_training_shield_template.Durability = 100;
				small_training_shield_template.MaxDurability = 100;
				small_training_shield_template.Condition = 50000;
				small_training_shield_template.MaxCondition = 50000;
				small_training_shield_template.Quality = 100;
				small_training_shield_template.DPS_AF = 10;
				small_training_shield_template.SPD_ABS = 10;
				small_training_shield_template.Hand = 2;
				small_training_shield_template.Type_Damage = 1;
				small_training_shield_template.Object_Type = 0x2A;
				small_training_shield_template.Item_Type = 11;
				small_training_shield_template.Weight = 32;
				small_training_shield_template.Model = 59;
				small_training_shield_template.IsPickable = true;
				small_training_shield_template.IsDropable = false;
				small_training_shield_template.Id_nb = "small_training_shield";
				GameServer.Database.AddNewObject(small_training_shield_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + small_training_shield_template.Id_nb);
			}
			ItemTemplate training_shield_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "training_shield");
			if (training_shield_template == null)
			{
				training_shield_template = new ItemTemplate();
				training_shield_template.Name = "training shield";
				training_shield_template.Level = 2;
				training_shield_template.Durability = 100;
				training_shield_template.MaxDurability = 100;
				training_shield_template.Condition = 50000;
				training_shield_template.MaxCondition = 50000;
				training_shield_template.Quality = 90;
				training_shield_template.DPS_AF = 1;
				training_shield_template.SPD_ABS = 1;
				training_shield_template.Hand = 2;
				training_shield_template.Type_Damage = 1;
				training_shield_template.Object_Type = 0x2A;
				training_shield_template.Item_Type = 11;
				training_shield_template.Weight = 32;
				training_shield_template.Model = 59;
				training_shield_template.IsPickable = true;
				training_shield_template.IsDropable = false;
				training_shield_template.Id_nb = "training_shield";
				GameServer.Database.AddNewObject(training_shield_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + training_shield_template.Id_nb);
			}
			#region Respec Stones
			#region Respec Realm
			ItemTemplate respec_realm = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "respec_realm");
			if (respec_realm == null)
			{
				respec_realm = new ItemTemplate();
				respec_realm.Name = "Luminescent Abrogo Stone";
				respec_realm.Level = 80;
				respec_realm.Durability = 80;
				respec_realm.MaxDurability = 100;
				respec_realm.Condition = 50000;
				respec_realm.MaxCondition = 50000;
				respec_realm.Quality = 70;
				respec_realm.DPS_AF = 0;
				respec_realm.SPD_ABS = 0;
				respec_realm.Hand = 0;
				respec_realm.Type_Damage = 0;
				respec_realm.Object_Type = (int)eObjectType.Magical;
				respec_realm.Item_Type = 0;
				respec_realm.Weight = 10;
				respec_realm.Model = 514;
				respec_realm.IsPickable = true;
				respec_realm.IsDropable = true;
				respec_realm.Id_nb = "respec_realm";
				GameServer.Database.AddNewObject(respec_realm);
				if (log.IsDebugEnabled)
					log.Debug("Added " + respec_realm.Id_nb);
			}
			#endregion
			#region Respec Single
			ItemTemplate respec_single = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "respec_single");
			if (respec_single == null)
			{
				respec_single = new ItemTemplate();
				respec_single.Name = "Luminescent Ceriac Stone";
				respec_single.Level = 80;
				respec_single.Durability = 80;
				respec_single.MaxDurability = 100;
				respec_single.Condition = 50000;
				respec_single.MaxCondition = 50000;
				respec_single.Quality = 70;
				respec_single.DPS_AF = 0;
				respec_single.SPD_ABS = 0;
				respec_single.Hand = 0;
				respec_single.Type_Damage = 0;
				respec_single.Object_Type = (int)eObjectType.Magical;
				respec_single.Item_Type = 0;
				respec_single.Weight = 10;
				respec_single.Model = 514;
				respec_single.IsPickable = true;
				respec_single.IsDropable = true;
				respec_single.Id_nb = "respec_single";
				GameServer.Database.AddNewObject(respec_single);
				if (log.IsDebugEnabled)
					log.Debug("Added " + respec_single.Id_nb);
			}
			#endregion
			#region Respec Full
			ItemTemplate respec_full = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "respec_full");
			if (respec_full == null)
			{
				respec_full = new ItemTemplate();
				respec_full.Name = "Luminescent Exerpise Stone";
				respec_full.Level = 80;
				respec_full.Durability = 80;
				respec_full.MaxDurability = 100;
				respec_full.Condition = 50000;
				respec_full.MaxCondition = 50000;
				respec_full.Quality = 70;
				respec_full.DPS_AF = 0;
				respec_full.SPD_ABS = 0;
				respec_full.Hand = 0;
				respec_full.Type_Damage = 0;
				respec_full.Object_Type = (int)eObjectType.Magical;
				respec_full.Item_Type = 0;
				respec_full.Weight = 10;
				respec_full.Model = 514;
				respec_full.IsPickable = true;
				respec_full.IsDropable = true;
				respec_full.Id_nb = "respec_full";
				GameServer.Database.AddNewObject(respec_full);
				if (log.IsDebugEnabled)
					log.Debug("Added " + respec_full.Id_nb);
			}
			#endregion
			#endregion
		}
	}
}