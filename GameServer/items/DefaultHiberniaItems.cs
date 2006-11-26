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
using System.Reflection;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Items
{
	/// <summary>
	/// Creates Level 5 items given by trainer for Hibernia
	/// </summary>
	public class DefaultHiberniaItems
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args)
		{
			ItemTemplate animist_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "animist_item");
			if (animist_item_template == null)
			{
				animist_item_template = new ItemTemplate();
				animist_item_template.Name = "Staff of the Aborical";
				animist_item_template.Level = 5;
				animist_item_template.Durability = 100;
				animist_item_template.MaxDurability = 100;
				animist_item_template.Condition = 50000;
				animist_item_template.MaxCondition = 50000;
				animist_item_template.Quality = 90;
				animist_item_template.MaxQuality = 90;
				animist_item_template.DPS_AF = 30;
				animist_item_template.SPD_ABS = 44;
				animist_item_template.Hand = 0;
				animist_item_template.Type_Damage = 1;
				animist_item_template.Object_Type = 8;
				animist_item_template.Item_Type = 12;
				animist_item_template.Weight = 45;
				animist_item_template.Model = 19;
				animist_item_template.Realm = (int)eRealm.Hibernia;
				animist_item_template.IsPickable = true;
				animist_item_template.IsDropable = false;
				animist_item_template.Id_nb = "animist_item";

				//TODO findout what sun AND moon stands for
				animist_item_template.Bonus1 = 4;
				animist_item_template.Bonus1Type = (int) eProperty.Focus_Arboreal;

				animist_item_template.Bonus2 = 4;
				animist_item_template.Bonus2Type = (int) eProperty.Focus_CreepingPath;
				//
				animist_item_template.Bonus3 = 4;
				animist_item_template.Bonus3Type = (int) eProperty.Focus_Verdant;

				GameServer.Database.AddNewObject(animist_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + animist_item_template.Id_nb);
			}
			ItemTemplate bard_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "bard_item");
			if (bard_item_template == null)
			{
				bard_item_template = new ItemTemplate();
				bard_item_template.Name = "Bard Initiate Lute";
				bard_item_template.Level = 5;
				bard_item_template.Durability = 100;
				bard_item_template.MaxDurability = 100;
				bard_item_template.Condition = 50000;
				bard_item_template.MaxCondition = 50000;
				bard_item_template.Quality = 90;
				bard_item_template.MaxQuality = 90;
				bard_item_template.DPS_AF = (int)eInstrumentType.Lute;
				bard_item_template.SPD_ABS = 40;
				bard_item_template.Hand = 1;
				bard_item_template.Type_Damage = 0;
				bard_item_template.Object_Type = (int)eObjectType.Instrument;
				bard_item_template.Item_Type = 12;
				bard_item_template.Weight = 45;
				bard_item_template.Model = 227;
				bard_item_template.Realm = (int)eRealm.Hibernia;
				bard_item_template.IsPickable = true;
				bard_item_template.IsDropable = false;
				bard_item_template.Id_nb = "bard_item";

				bard_item_template.Bonus1 = 1;
				bard_item_template.Bonus1Type = (int) eProperty.Skill_Instruments;

				GameServer.Database.AddNewObject(bard_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + bard_item_template.Id_nb);
			}
			ItemTemplate blademaster_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "blademaster_item");
			if (blademaster_item_template == null)
			{
				blademaster_item_template = new ItemTemplate();
				blademaster_item_template.Name = "Blademaster Initiate Gloves";
				blademaster_item_template.Level = 5;
				blademaster_item_template.Durability = 100;
				blademaster_item_template.MaxDurability = 100;
				blademaster_item_template.Condition = 50000;
				blademaster_item_template.MaxCondition = 50000;
				blademaster_item_template.Quality = 90;
				blademaster_item_template.MaxQuality = 90;
				blademaster_item_template.DPS_AF = 14;
				blademaster_item_template.SPD_ABS = 19;
				blademaster_item_template.Object_Type = (int) eObjectType.Reinforced;
				blademaster_item_template.Item_Type = (int) eEquipmentItems.HAND;
				blademaster_item_template.Weight = 16;
				blademaster_item_template.Model = 346;
				blademaster_item_template.Realm = (int)eRealm.Hibernia;
				blademaster_item_template.IsPickable = true;
				blademaster_item_template.IsDropable = false;
				blademaster_item_template.Id_nb = "blademaster_item";

				blademaster_item_template.Bonus1 = 1;
				blademaster_item_template.Bonus1Type = (int) eStat.QUI;

				GameServer.Database.AddNewObject(blademaster_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + blademaster_item_template.Id_nb);
			}
			ItemTemplate champion_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "champion_item");
			if (champion_item_template == null)
			{
				champion_item_template = new ItemTemplate();
				champion_item_template.Name = "Champion Initiate Gauntlets";
				champion_item_template.Level = 5;
				champion_item_template.Durability = 100;
				champion_item_template.MaxDurability = 100;
				champion_item_template.Condition = 50000;
				champion_item_template.MaxCondition = 50000;
				champion_item_template.Quality = 90;
				champion_item_template.MaxQuality = 90;
				champion_item_template.DPS_AF = 14;
				champion_item_template.SPD_ABS = 19;
				champion_item_template.Object_Type = (int) eObjectType.Reinforced;
				champion_item_template.Item_Type = (int) eEquipmentItems.HAND;
				champion_item_template.Weight = 24;
				champion_item_template.Model = 346;
				champion_item_template.Realm = (int)eRealm.Hibernia;
				champion_item_template.IsPickable = true;
				champion_item_template.IsDropable = false;
				champion_item_template.Id_nb = "champion_item";

				champion_item_template.Bonus1 = 1;
				champion_item_template.Bonus1Type = (int) eStat.DEX;

				GameServer.Database.AddNewObject(champion_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + champion_item_template.Id_nb);
			}
			ItemTemplate hero_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "hero_item");
			if (hero_item_template == null)
			{
				hero_item_template = new ItemTemplate();
				hero_item_template.Name = "Hero Initiate Gauntlets";
				hero_item_template.Level = 5;
				hero_item_template.Durability = 100;
				hero_item_template.MaxDurability = 100;
				hero_item_template.Condition = 50000;
				hero_item_template.MaxCondition = 50000;
				hero_item_template.Quality = 90;
				hero_item_template.MaxQuality = 90;
				hero_item_template.DPS_AF = 14;
				hero_item_template.SPD_ABS = 19;
				hero_item_template.Object_Type = (int) eObjectType.Reinforced;
				hero_item_template.Item_Type = (int) eEquipmentItems.HAND;
				hero_item_template.Weight = 24;
				hero_item_template.Model = 346;
				hero_item_template.Realm = (int)eRealm.Hibernia;
				hero_item_template.IsPickable = true;
				hero_item_template.IsDropable = false;
				hero_item_template.Id_nb = "hero_item";

				hero_item_template.Bonus1 = 1;
				hero_item_template.Bonus1Type = (int) eStat.STR;

				GameServer.Database.AddNewObject(hero_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + hero_item_template.Id_nb);
			}
			ItemTemplate druid_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "druid_item");
			if (druid_item_template == null)
			{
				druid_item_template = new ItemTemplate();
				druid_item_template.Name = "Druid Initiate Gloves";
				druid_item_template.Level = 5;
				druid_item_template.Durability = 100;
				druid_item_template.MaxDurability = 100;
				druid_item_template.Condition = 50000;
				druid_item_template.MaxCondition = 50000;
				druid_item_template.Quality = 90;
				druid_item_template.MaxQuality = 90;
				druid_item_template.DPS_AF = 14;
				druid_item_template.SPD_ABS = 10;
				druid_item_template.Object_Type = (int) eObjectType.Leather;
				druid_item_template.Item_Type = (int) eEquipmentItems.HAND;
				druid_item_template.Weight = 16;
				druid_item_template.Model = 356;
				druid_item_template.Realm = (int)eRealm.Hibernia;
				druid_item_template.IsPickable = true;
				druid_item_template.IsDropable = false;
				druid_item_template.Id_nb = "druid_item";

				druid_item_template.Bonus1 = 1;
				druid_item_template.Bonus1Type = (int) eStat.EMP;

				GameServer.Database.AddNewObject(druid_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + druid_item_template.Id_nb);
			}
			ItemTemplate warden_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "warden_item");
			if (warden_item_template == null)
			{
				warden_item_template = new ItemTemplate();
				warden_item_template.Name = "Warden Initiate Boots";
				warden_item_template.Level = 5;
				warden_item_template.Durability = 100;
				warden_item_template.MaxDurability = 100;
				warden_item_template.Condition = 50000;
				warden_item_template.MaxCondition = 50000;
				warden_item_template.Quality = 90;
				warden_item_template.MaxQuality = 90;
				warden_item_template.DPS_AF = 14;
				warden_item_template.SPD_ABS = 10;
				warden_item_template.Object_Type = (int) eObjectType.Leather;
				warden_item_template.Item_Type = (int) eEquipmentItems.FEET;
				warden_item_template.Weight = 16;
				warden_item_template.Model = 357;
				warden_item_template.Realm = (int)eRealm.Hibernia;
				warden_item_template.IsPickable = true;
				warden_item_template.IsDropable = false;
				warden_item_template.Id_nb = "warden_item";

				warden_item_template.Bonus1 = 1;
				warden_item_template.Bonus1Type = (int) eStat.QUI;

				GameServer.Database.AddNewObject(warden_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + warden_item_template.Id_nb);
			}
			ItemTemplate nightshade_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "nightshade_item");
			if (nightshade_item_template == null)
			{
				nightshade_item_template = new ItemTemplate();
				nightshade_item_template.Name = "Nightshade Initiate Gloves";
				nightshade_item_template.Level = 5;
				nightshade_item_template.Durability = 100;
				nightshade_item_template.MaxDurability = 100;
				nightshade_item_template.Condition = 50000;
				nightshade_item_template.MaxCondition = 50000;
				nightshade_item_template.Quality = 90;
				nightshade_item_template.MaxQuality = 90;
				nightshade_item_template.DPS_AF = 14;
				nightshade_item_template.SPD_ABS = 10;
				nightshade_item_template.Hand = 0;
				nightshade_item_template.Type_Damage = 0;
				nightshade_item_template.Object_Type = 33;
				nightshade_item_template.Item_Type = 22;
				nightshade_item_template.Weight = 16;
				nightshade_item_template.Model = 356;
				nightshade_item_template.Realm = (int)eRealm.Hibernia;
				nightshade_item_template.IsPickable = true;
				nightshade_item_template.IsDropable = false;
				nightshade_item_template.Id_nb = "nightshade_item";
				GameServer.Database.AddNewObject(nightshade_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + nightshade_item_template.Id_nb);
			}
			ItemTemplate ranger_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "ranger_item");
			if (ranger_item_template == null)
			{
				ranger_item_template = new ItemTemplate();
				ranger_item_template.Name = "Elven Recurve Bow";
				ranger_item_template.Level = 5;
				ranger_item_template.Durability = 100;
				ranger_item_template.MaxDurability = 100;
				ranger_item_template.Condition = 50000;
				ranger_item_template.MaxCondition = 50000;
				ranger_item_template.Quality = 90;
				ranger_item_template.MaxQuality = 90;
				ranger_item_template.DPS_AF = 30;
				ranger_item_template.SPD_ABS = 47;
				ranger_item_template.Hand = 0;
				ranger_item_template.Type_Damage = (int) eDamageType.Thrust;
				ranger_item_template.Object_Type = (int) eObjectType.RecurvedBow;
				ranger_item_template.Item_Type = (int) eEquipmentItems.RANGED;
				ranger_item_template.Weight = 31;
				ranger_item_template.Model = 471;
				ranger_item_template.Realm = (int)eRealm.Hibernia;
				ranger_item_template.IsPickable = true;
				ranger_item_template.IsDropable = false;
				ranger_item_template.Id_nb = "ranger_item";

				ranger_item_template.Bonus1 = 1;
				ranger_item_template.Bonus1Type = (int) eProperty.Skill_RecurvedBow;

				GameServer.Database.AddNewObject(ranger_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + ranger_item_template.Id_nb);
			}
			ItemTemplate valewalker_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "valewalker_item");
			if (valewalker_item_template == null)
			{
				valewalker_item_template = new ItemTemplate();
				valewalker_item_template.Name = "Scythe of the Initiate";
				valewalker_item_template.Level = 5;
				valewalker_item_template.Durability = 100;
				valewalker_item_template.MaxDurability = 100;
				valewalker_item_template.Condition = 50000;
				valewalker_item_template.MaxCondition = 50000;
				valewalker_item_template.Quality = 90;
				valewalker_item_template.MaxQuality = 90;
				valewalker_item_template.DPS_AF = 26;
				valewalker_item_template.SPD_ABS = 42;
				valewalker_item_template.Hand = 0;
				valewalker_item_template.Type_Damage = (int) eDamageType.Slash;
				valewalker_item_template.Object_Type = (int) eObjectType.Scythe;
				valewalker_item_template.Item_Type = (int) eEquipmentItems.TWO_HANDED;
				valewalker_item_template.Weight = 35;
				valewalker_item_template.Model = 929;
				valewalker_item_template.Realm = (int)eRealm.Hibernia;
				valewalker_item_template.IsPickable = true;
				valewalker_item_template.IsDropable = false;
				valewalker_item_template.Id_nb = "valewalker_item";

				valewalker_item_template.Bonus1 = 1;
				valewalker_item_template.Bonus1Type = (int) eProperty.Skill_Scythe;

				GameServer.Database.AddNewObject(valewalker_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + valewalker_item_template.Id_nb);
			}
			ItemTemplate eldritch_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "eldritch_item");
			if (eldritch_item_template == null)
			{
				eldritch_item_template = new ItemTemplate();
				eldritch_item_template.Name = "Eldritch Staff of Focus";
				eldritch_item_template.Level = 5;
				eldritch_item_template.Durability = 100;
				eldritch_item_template.MaxDurability = 100;
				eldritch_item_template.Condition = 50000;
				eldritch_item_template.MaxCondition = 50000;
				eldritch_item_template.Quality = 90;
				eldritch_item_template.MaxQuality = 90;
				eldritch_item_template.DPS_AF = 30;
				eldritch_item_template.SPD_ABS = 44;
				eldritch_item_template.Hand = 0;
				eldritch_item_template.Type_Damage = 1;
				eldritch_item_template.Object_Type = 8;
				eldritch_item_template.Item_Type = 12;
				eldritch_item_template.Weight = 45;
				eldritch_item_template.Model = 19;
				eldritch_item_template.Realm = (int)eRealm.Hibernia;
				eldritch_item_template.IsPickable = true;
				eldritch_item_template.IsDropable = false;
				eldritch_item_template.Id_nb = "eldritch_item";

				eldritch_item_template.Bonus1 = 4;
				eldritch_item_template.Bonus1Type = (int) eProperty.Focus_Void;

				//eldritch_item_template.Bonus2=4;
				//eldritch_item_template.Bonus2Type= (int) eProperty.Focus_;

				//eldritch_item_template.Bonus3=4;
				//eldritch_item_template.Bonus3Type= (int) eProperty.Focus_; 
				GameServer.Database.AddNewObject(eldritch_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + eldritch_item_template.Id_nb);
			}
			ItemTemplate enchanter_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "enchanter_item");
			if (enchanter_item_template == null)
			{
				enchanter_item_template = new ItemTemplate();
				enchanter_item_template.Name = "Enchanter Staff of Focus";
				enchanter_item_template.Level = 5;
				enchanter_item_template.Durability = 100;
				enchanter_item_template.MaxDurability = 100;
				enchanter_item_template.Condition = 50000;
				enchanter_item_template.MaxCondition = 50000;
				enchanter_item_template.Quality = 90;
				enchanter_item_template.MaxQuality = 90;
				enchanter_item_template.DPS_AF = 30;
				enchanter_item_template.SPD_ABS = 44;
				enchanter_item_template.Hand = 0;
				enchanter_item_template.Type_Damage = 1;
				enchanter_item_template.Object_Type = 8;
				enchanter_item_template.Item_Type = 12;
				enchanter_item_template.Weight = 45;
				enchanter_item_template.Model = 19;
				enchanter_item_template.Realm = (int)eRealm.Hibernia;
				enchanter_item_template.IsPickable = true;
				enchanter_item_template.IsDropable = false;
				enchanter_item_template.Id_nb = "enchanter_item";

				enchanter_item_template.Bonus1 = 4;
				enchanter_item_template.Bonus1Type = (int) eProperty.Focus_Enchantments;

				//enchanter_item_template.Bonus2=4;
				//enchanter_item_template.Bonus2Type= (int) eProperty.Focus_;

				//enchanter_item_template.Bonus3=4;
				//enchanter_item_template.Bonus3Type= (int) eProperty.Focus_; 

				GameServer.Database.AddNewObject(enchanter_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + enchanter_item_template.Id_nb);
			}
			ItemTemplate mentalist_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), "mentalist_item");
			if (mentalist_item_template == null)
			{
				mentalist_item_template = new ItemTemplate();
				mentalist_item_template.Name = "Staff of Mental Clarity";
				mentalist_item_template.Level = 5;
				mentalist_item_template.Durability = 100;
				mentalist_item_template.MaxDurability = 100;
				mentalist_item_template.Condition = 50000;
				mentalist_item_template.MaxCondition = 50000;
				mentalist_item_template.Quality = 90;
				mentalist_item_template.MaxQuality = 90;
				mentalist_item_template.DPS_AF = 30;
				mentalist_item_template.SPD_ABS = 44;
				mentalist_item_template.Hand = 0;
				mentalist_item_template.Type_Damage = (int) eDamageType.Crush;
				mentalist_item_template.Object_Type = 8;
				mentalist_item_template.Item_Type = 12;
				mentalist_item_template.Weight = 45;
				mentalist_item_template.Model = 19;
				mentalist_item_template.Realm = (int)eRealm.Hibernia;
				mentalist_item_template.IsPickable = true;
				mentalist_item_template.IsDropable = false;
				mentalist_item_template.Id_nb = "mentalist_item";

				//TODO findout what sun AND moon stands for
				//mentalist_item_template.Bonus3=4;
				//mentalist_item_template.Bonus3Type= (int) eProperty.Focus_Mind; 

				//mentalist_item_template.Bonus2=4;
				//mentalist_item_template.Bonus2Type= (int) eProperty.Focus_Light;
				//
				mentalist_item_template.Bonus1 = 4;
				mentalist_item_template.Bonus1Type = (int) eProperty.Focus_Mentalism;

				GameServer.Database.AddNewObject(mentalist_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added " + mentalist_item_template.Id_nb);
			}
		}
	}
}