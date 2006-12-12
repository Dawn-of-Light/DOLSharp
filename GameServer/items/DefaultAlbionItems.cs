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
using DOL.Events;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Items
{
	/// <summary>
	/// Creates Level 5 items given by trainer for Albion
	/// </summary>
	public class DefaultAlbionItems
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[GameServerStartedEvent]
		public static void OnServerStartup(DOLEvent e, object sender, EventArgs args) 
		{
			ItemTemplate cabalist_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "cabalist_item");
			if (cabalist_item_template==null)
			{
				cabalist_item_template = new ItemTemplate();
				cabalist_item_template.Name = "Cabalist Staff of Focus";
				cabalist_item_template.Level = 5;
				cabalist_item_template.Durability=100;
				cabalist_item_template.MaxDurability=100;
				cabalist_item_template.Condition = 50000;
				cabalist_item_template.MaxCondition = 50000;
				cabalist_item_template.Quality = 90;
				cabalist_item_template.MaxQuality = 90;
				cabalist_item_template.DPS_AF = 30;
				cabalist_item_template.SPD_ABS = 44;
				cabalist_item_template.Hand = 0;
				cabalist_item_template.Type_Damage = 1;
				cabalist_item_template.Object_Type = 8;
				cabalist_item_template.Item_Type = 12;
				cabalist_item_template.Weight = 45;
				cabalist_item_template.Model = 19;
				cabalist_item_template.Realm = (int)eRealm.Albion;
				cabalist_item_template.IsPickable = true; 
				cabalist_item_template.IsDropable = false; 
				cabalist_item_template.Id_nb = "cabalist_item";

				cabalist_item_template.Bonus1=4;
				cabalist_item_template.Bonus1Type= (int) eProperty.Focus_Body;

				cabalist_item_template.Bonus2=4;
				cabalist_item_template.Bonus2Type= (int) eProperty.Focus_Matter;

				cabalist_item_template.Bonus3=4;
				cabalist_item_template.Bonus3Type= (int) eProperty.Focus_Spirit;

				GameServer.Database.AddNewObject(cabalist_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+cabalist_item_template.Id_nb);
			}
			ItemTemplate wizard_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "wizard_item");
			if (wizard_item_template==null)
			{
				wizard_item_template = new ItemTemplate();
				wizard_item_template.Name = "Wizard Staff of Focus";
				wizard_item_template.Level = 5;
				wizard_item_template.Durability=100;
				wizard_item_template.MaxDurability=100;
				wizard_item_template.Condition = 50000;
				wizard_item_template.MaxCondition = 50000;
				wizard_item_template.Quality = 90;
				wizard_item_template.MaxQuality = 90;
				wizard_item_template.DPS_AF = 30;
				wizard_item_template.SPD_ABS = 44;
				wizard_item_template.Hand = 0;
				wizard_item_template.Type_Damage = 1;
				wizard_item_template.Object_Type = 8;
				wizard_item_template.Item_Type = 12;
				wizard_item_template.Weight = 45;
				wizard_item_template.Model = 19;
				wizard_item_template.Realm = (int)eRealm.Albion;
				wizard_item_template.IsPickable = true; 
				wizard_item_template.IsDropable = false; 
				wizard_item_template.Id_nb = "wizard_item";

				wizard_item_template.Bonus1=4;
				wizard_item_template.Bonus1Type= (int) eProperty.Focus_Earth;

				wizard_item_template.Bonus2=4;
				wizard_item_template.Bonus2Type= (int) eProperty.Focus_Cold;

				wizard_item_template.Bonus3=4;
				wizard_item_template.Bonus3Type= (int) eProperty.Focus_Fire;

				GameServer.Database.AddNewObject(wizard_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+wizard_item_template.Id_nb);
			}
			ItemTemplate theurgist_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "theurgist_item");
			if (theurgist_item_template==null)
			{
				theurgist_item_template = new ItemTemplate();
				theurgist_item_template.Name = "Theurgist Staff of Focus";
				theurgist_item_template.Level = 5;
				theurgist_item_template.Durability=100;
				theurgist_item_template.MaxDurability=100;
				theurgist_item_template.Condition = 50000;
				theurgist_item_template.MaxCondition = 50000;
				theurgist_item_template.Quality = 90;
				theurgist_item_template.MaxQuality = 90;
				theurgist_item_template.DPS_AF = 30;
				theurgist_item_template.SPD_ABS = 44;
				theurgist_item_template.Hand = 0;
				theurgist_item_template.Type_Damage = 1;
				theurgist_item_template.Object_Type = 8;
				theurgist_item_template.Item_Type = 12;
				theurgist_item_template.Weight = 45;
				theurgist_item_template.Model = 19;
				theurgist_item_template.Realm = (int)eRealm.Albion;
				theurgist_item_template.IsPickable = true; 
				theurgist_item_template.IsDropable = false; 
				theurgist_item_template.Id_nb = "theurgist_item";

				theurgist_item_template.Bonus1=4;
				theurgist_item_template.Bonus1Type= (int) eProperty.Focus_Earth;

				theurgist_item_template.Bonus2=4;
				theurgist_item_template.Bonus2Type= (int) eProperty.Focus_Cold;

				theurgist_item_template.Bonus3=4;
				theurgist_item_template.Bonus3Type= (int) eProperty.Focus_Air;

				GameServer.Database.AddNewObject(theurgist_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+theurgist_item_template.Id_nb);
			}
			ItemTemplate sorcerer_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "sorcerer_item");
			if (sorcerer_item_template==null)
			{
				sorcerer_item_template = new ItemTemplate();
				sorcerer_item_template.Name = "Sorcerer Staff of Focus";
				sorcerer_item_template.Level = 5;
				sorcerer_item_template.Durability=100;
				sorcerer_item_template.MaxDurability=100;
				sorcerer_item_template.Condition = 50000;
				sorcerer_item_template.MaxCondition = 50000;
				sorcerer_item_template.Quality = 90;
				sorcerer_item_template.MaxQuality = 90;
				sorcerer_item_template.DPS_AF = 30;
				sorcerer_item_template.SPD_ABS = 44;
				sorcerer_item_template.Hand = 0;
				sorcerer_item_template.Type_Damage = 1;
				sorcerer_item_template.Object_Type = 8;
				sorcerer_item_template.Item_Type = 12;
				sorcerer_item_template.Weight = 45;
				sorcerer_item_template.Model = 19;
				sorcerer_item_template.Realm = (int)eRealm.Albion;
				sorcerer_item_template.IsPickable = true; 
				sorcerer_item_template.IsDropable = false; 
				sorcerer_item_template.Id_nb = "sorcerer_item";
				
				sorcerer_item_template.Bonus1=4;
				sorcerer_item_template.Bonus1Type= (int) eProperty.Focus_Matter;

				sorcerer_item_template.Bonus2=4;
				sorcerer_item_template.Bonus2Type= (int) eProperty.Focus_Body;

				sorcerer_item_template.Bonus3=4;
				sorcerer_item_template.Bonus3Type= (int) eProperty.Focus_Mind;

				GameServer.Database.AddNewObject(sorcerer_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+sorcerer_item_template.Id_nb);
			}
			ItemTemplate necromancer_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "necromancer_item");
			if (necromancer_item_template==null)
			{
				necromancer_item_template = new ItemTemplate();
				necromancer_item_template.Name = "Necromancer's Staff";
				necromancer_item_template.Level = 5;
				necromancer_item_template.Durability=100;
				necromancer_item_template.MaxDurability=100;
				necromancer_item_template.Condition = 50000;
				necromancer_item_template.MaxCondition = 50000;
				necromancer_item_template.Quality = 90;
				necromancer_item_template.MaxQuality = 90;
				necromancer_item_template.DPS_AF = 30;
				necromancer_item_template.SPD_ABS = 44;
				necromancer_item_template.Hand = 0;
				necromancer_item_template.Type_Damage = 1;
				necromancer_item_template.Object_Type = 8;
				necromancer_item_template.Item_Type = 12;
				necromancer_item_template.Weight = 45;
				necromancer_item_template.Model = 821;
				necromancer_item_template.Realm = (int)eRealm.Albion;
				necromancer_item_template.IsPickable = true; 
				necromancer_item_template.IsDropable = false; 
				necromancer_item_template.Id_nb = "necromancer_item";

				necromancer_item_template.Bonus1=4;
				necromancer_item_template.Bonus1Type= (int) eProperty.Focus_DeathSight;

				necromancer_item_template.Bonus2=4;
				necromancer_item_template.Bonus2Type= (int) eProperty.Focus_PainWorking;

				necromancer_item_template.Bonus3=4;
				necromancer_item_template.Bonus3Type= (int) eProperty.Focus_DeathServant;

				GameServer.Database.AddNewObject(necromancer_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+necromancer_item_template.Id_nb);
			}
			ItemTemplate slash_sword_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "slash_sword_item");
			if (slash_sword_item_template==null)
			{
				slash_sword_item_template = new ItemTemplate();
				slash_sword_item_template.Name = "Sword of the Initiate";
				slash_sword_item_template.Level = 5;
				slash_sword_item_template.Durability=100;
				slash_sword_item_template.MaxDurability=100;
				slash_sword_item_template.Condition = 50000;
				slash_sword_item_template.MaxCondition = 50000;
				slash_sword_item_template.Quality = 90;
				slash_sword_item_template.MaxQuality = 90;
				slash_sword_item_template.DPS_AF = 30;
				slash_sword_item_template.SPD_ABS = 25;
				slash_sword_item_template.Hand = 2;
				slash_sword_item_template.Type_Damage = 2;
				slash_sword_item_template.Object_Type = 3;
				slash_sword_item_template.Item_Type = 10;
				slash_sword_item_template.Weight = 20;
				slash_sword_item_template.Model = 3;
				slash_sword_item_template.Realm = (int)eRealm.Albion;
				slash_sword_item_template.IsPickable = true; 
				slash_sword_item_template.IsDropable = false; 
				slash_sword_item_template.Id_nb = "slash_sword_item";

				slash_sword_item_template.Bonus1=1;
				slash_sword_item_template.Bonus1Type= (int) eProperty.Skill_Slashing;

				GameServer.Database.AddNewObject(slash_sword_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+slash_sword_item_template.Id_nb);
			}
			ItemTemplate crush_sword_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "crush_sword_item");
			if (crush_sword_item_template==null)
			{
				crush_sword_item_template = new ItemTemplate();
				crush_sword_item_template.Name = "Mace of the Initiate";
				crush_sword_item_template.Level = 5;
				crush_sword_item_template.Durability=100;
				crush_sword_item_template.MaxDurability=100;
				crush_sword_item_template.Condition = 50000;
				crush_sword_item_template.MaxCondition = 50000;
				crush_sword_item_template.Quality = 90;
				crush_sword_item_template.MaxQuality = 90;
				crush_sword_item_template.DPS_AF = 30;
				crush_sword_item_template.SPD_ABS = 30;
				crush_sword_item_template.Hand = 2;
				crush_sword_item_template.Type_Damage = 1;
				crush_sword_item_template.Object_Type = 2;
				crush_sword_item_template.Item_Type = 10;
				crush_sword_item_template.Weight = 32;
				crush_sword_item_template.Model = 13;
				crush_sword_item_template.Realm = (int)eRealm.Albion;
				crush_sword_item_template.IsPickable = true; 
				crush_sword_item_template.IsDropable = false; 
				crush_sword_item_template.Id_nb = "crush_sword_item";

				crush_sword_item_template.Bonus1=1;
				crush_sword_item_template.Bonus1Type= (int) eProperty.Skill_Rejuvenation;

				GameServer.Database.AddNewObject(crush_sword_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+crush_sword_item_template.Id_nb);
			}
			ItemTemplate thrust_sword_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "thrust_sword_item");
			if (thrust_sword_item_template==null)
			{
				thrust_sword_item_template = new ItemTemplate();
				thrust_sword_item_template.Name = "Rapier of the Initiate";
				thrust_sword_item_template.Level = 5;
				thrust_sword_item_template.Durability=100;
				thrust_sword_item_template.MaxDurability=100;
				thrust_sword_item_template.Condition = 50000;
				thrust_sword_item_template.MaxCondition = 50000;
				thrust_sword_item_template.Quality = 90;
				thrust_sword_item_template.MaxQuality = 90;
				thrust_sword_item_template.DPS_AF = 30;
				thrust_sword_item_template.SPD_ABS = 25;
				thrust_sword_item_template.Hand = 2;
				thrust_sword_item_template.Type_Damage = 3;
				thrust_sword_item_template.Object_Type = 4;
				thrust_sword_item_template.Item_Type = 10;
				thrust_sword_item_template.Weight = 10;
				thrust_sword_item_template.Model = 21;
				thrust_sword_item_template.Realm = (int)eRealm.Albion;
				thrust_sword_item_template.IsPickable = true; 
				thrust_sword_item_template.IsDropable = false; 
				thrust_sword_item_template.Id_nb = "thrust_sword_item";

				thrust_sword_item_template.Bonus1=1;
				thrust_sword_item_template.Bonus1Type= (int) eProperty.Skill_Thrusting;

				GameServer.Database.AddNewObject(thrust_sword_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+thrust_sword_item_template.Id_nb);
			}
			ItemTemplate twohand_sword_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "twohand_sword_item");
			if (twohand_sword_item_template==null)
			{
				twohand_sword_item_template = new ItemTemplate();
				twohand_sword_item_template.Name = "Greatsword of the Initiate";
				twohand_sword_item_template.Level = 5;
				twohand_sword_item_template.Durability=100;
				twohand_sword_item_template.MaxDurability=100;
				twohand_sword_item_template.Condition = 50000;
				twohand_sword_item_template.MaxCondition = 50000;
				twohand_sword_item_template.Quality = 90;
				twohand_sword_item_template.MaxQuality = 90;
				twohand_sword_item_template.DPS_AF = 30;
				twohand_sword_item_template.SPD_ABS = 47;
				twohand_sword_item_template.Hand = 0;
				twohand_sword_item_template.Type_Damage = 2;
				twohand_sword_item_template.Object_Type = 6;
				twohand_sword_item_template.Item_Type = 12;
				twohand_sword_item_template.Weight = 55;
				twohand_sword_item_template.Model = 7;
				twohand_sword_item_template.Realm = (int)eRealm.Albion;
				twohand_sword_item_template.IsPickable = true; 
				twohand_sword_item_template.IsDropable = false; 
				twohand_sword_item_template.Id_nb = "twohand_sword_item";

				twohand_sword_item_template.Bonus1=1;
				twohand_sword_item_template.Bonus1Type= (int) eProperty.Skill_Two_Handed;

				GameServer.Database.AddNewObject(twohand_sword_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+twohand_sword_item_template.Id_nb);
			}
			ItemTemplate pike_polearm_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "pike_polearm_item");
			if (pike_polearm_item_template==null)
			{
				pike_polearm_item_template = new ItemTemplate();
				pike_polearm_item_template.Name = "Pike of the Initiate";
				pike_polearm_item_template.Level = 5;
				pike_polearm_item_template.Durability=100;
				pike_polearm_item_template.MaxDurability=100;
				pike_polearm_item_template.Condition = 50000;
				pike_polearm_item_template.MaxCondition = 50000;
				pike_polearm_item_template.Quality = 90;
				pike_polearm_item_template.MaxQuality = 90;
				pike_polearm_item_template.DPS_AF = 32;
				pike_polearm_item_template.SPD_ABS = 30;
				pike_polearm_item_template.Hand = 0;
				pike_polearm_item_template.Type_Damage = 3;
				pike_polearm_item_template.Object_Type = 7;
				pike_polearm_item_template.Item_Type = 12;
				pike_polearm_item_template.Weight = 44;
				pike_polearm_item_template.Model = 69;
				pike_polearm_item_template.Realm = (int)eRealm.Albion;
				pike_polearm_item_template.IsPickable = true; 
				pike_polearm_item_template.IsDropable = false; 
				pike_polearm_item_template.Id_nb = "pike_polearm_item";

				pike_polearm_item_template.Bonus1=1;
				pike_polearm_item_template.Bonus1Type= (int) eProperty.Skill_Polearms;

				GameServer.Database.AddNewObject(pike_polearm_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+pike_polearm_item_template.Id_nb);
			}

			ItemTemplate scout_item_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "scout_item");
			if (scout_item_template==null)
			{
				scout_item_template = new ItemTemplate();
				scout_item_template.Name = "Huntsman's Longbow";
				scout_item_template.Level = 5;
				scout_item_template.Durability=100;
				scout_item_template.MaxDurability=100;
				scout_item_template.Condition = 50000;
				scout_item_template.MaxCondition = 50000;
				scout_item_template.Quality = 90;
				scout_item_template.MaxQuality = 90;
				scout_item_template.DPS_AF = 30;
				scout_item_template.SPD_ABS = 54;
				scout_item_template.Hand = 0;
				scout_item_template.Type_Damage = (int) eDamageType.Thrust;
				scout_item_template.Object_Type = (int) eObjectType.Longbow;
				scout_item_template.Item_Type = (int) eEquipmentItems.RANGED;
				scout_item_template.Weight = 31;
				scout_item_template.Model = 471;
				scout_item_template.Realm = (int)eRealm.Albion;
				scout_item_template.IsPickable = true; 
				scout_item_template.IsDropable = false; 
				scout_item_template.Id_nb = "scout_item";

				scout_item_template.Bonus1=1;
				scout_item_template.Bonus1Type= (int) eProperty.Skill_Long_bows;

				GameServer.Database.AddNewObject(scout_item_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+scout_item_template.Id_nb);
			}
		

			ItemTemplate friar_template = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof(ItemTemplate), "friar_item");
			if (friar_template==null)
			{
				friar_template = new ItemTemplate();
				friar_template.Name = "Robes of the Novice";
				friar_template.Level = 5;
				friar_template.Durability=100;
				friar_template.MaxDurability=100;
				friar_template.Condition = 50000;
				friar_template.MaxCondition = 50000;
				friar_template.Quality = 100;
				friar_template.MaxQuality = 100;
				friar_template.DPS_AF = 14;
				friar_template.SPD_ABS = 10;								
				friar_template.Object_Type = (int) eObjectType.Leather;
				friar_template.Item_Type = (int) eEquipmentItems.TORSO;
				friar_template.Weight = 15;
				friar_template.Model = 441;
				friar_template.Realm = (int)eRealm.Albion;
				friar_template.IsPickable = true; 
				friar_template.IsDropable = false; 
				friar_template.Id_nb = "friar_item";

				friar_template.Bonus1=1;
				friar_template.Bonus1Type= (int) eStat.DEX;

				friar_template.Bonus2=1;
				friar_template.Bonus2Type= (int) eStat.CON;				

				GameServer.Database.AddNewObject(friar_template);
				if (log.IsDebugEnabled)
					log.Debug("Added "+friar_template.Id_nb);
			}
		}
	}
}