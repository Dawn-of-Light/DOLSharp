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

using DOL.Events;
using DOL.Database;
using DOL.Database.Attributes;
using System.Collections;
using System.Collections.Generic;
using log4net;
using System.Reflection;
using DOL.GS;
using System.Threading;

namespace DOL.Database
{
	public class ConvertMgr
	{
		[GameServerStartedEvent]
		public static void ScriptLoaded(DOLEvent e, object sender, EventArgs args)
		{
			GameServer.Database.RegisterDataObject(typeof(OldItemTemplate));
			GameServer.Database.LoadDatabaseTable(typeof(OldItemTemplate));


			GameServer.Database.RegisterDataObject(typeof(OldInventoryItem));
			GameServer.Database.LoadDatabaseTable(typeof(OldInventoryItem));

			GameServer.Database.RegisterDataObject(typeof(OldDBSpell));
			GameServer.Database.LoadDatabaseTable(typeof(OldDBSpell));

			GameServer.Database.RegisterDataObject(typeof(OldDBLineXSpell));
			GameServer.Database.LoadDatabaseTable(typeof(OldDBLineXSpell));

			GameServer.Database.RegisterDataObject(typeof(OldDBStyleXSpell));
			GameServer.Database.LoadDatabaseTable(typeof(OldDBStyleXSpell));

			convertThread = new Thread(Convert);
			convertThread.Start();
			RegionTimer timer = new RegionTimer(WorldMgr.GetRegion(1).TimeManager);
			timer.Callback = CallBack;
			timer.Start(1);
		}

		private static Thread convertThread;
		private static int count = 0;
		private static int total = 0;
		private static DateTime convertStarted;
		private static List<string> excludes = new List<string>();
		private static List<int> m_spellIDs = new List<int>();

		private static void Convert()
		{

			//SPELL ID IS NOW A USHORT
			//get all spells with id > ushort.max
			OldDBSpell[] allSpells = (OldDBSpell[])GameServer.Database.SelectAllObjects(typeof(OldDBSpell));
			foreach (OldDBSpell spell in allSpells)
				m_spellIDs.Add(spell.SpellID);
			OldDBSpell[] spells = (OldDBSpell[])GameServer.Database.SelectObjects(typeof(OldDBSpell), "`SpellID` > '" + ushort.MaxValue + "'");
			GameServer.Instance.Logger.Info("Starting convert of " + spells.Length + " spells!");
			foreach (OldDBSpell spell in spells)
				ConvertSpell(spell);


			OldItemTemplate[] oldTemplates = (OldItemTemplate[])GameServer.Database.SelectAllObjects(typeof(OldItemTemplate));
			total = oldTemplates.Length;
			GameServer.Instance.Logger.Error("Beginning convert of " + total + " item templates!");
			convertStarted = DateTime.Now;

			foreach (OldItemTemplate old in oldTemplates)
			{
				if (old.ProcSpellID > ushort.MaxValue)
				{
					GameServer.Instance.Logger.Error(old.Name + " has a proc spell ID of " + old.ProcSpellID + " this is above 65535 max!");
					excludes.Add(old.Id_nb);
					continue;
				}

				if (old.ProcSpellID1 > ushort.MaxValue)
				{
					GameServer.Instance.Logger.Error(old.Name + " has a proc spell 1 ID of " + old.ProcSpellID1 + " this is above 65535 max!");
					excludes.Add(old.Id_nb);
					continue;
				}

				if (old.SpellID > ushort.MaxValue)
				{
					GameServer.Instance.Logger.Error(old.Name + " has a spell ID of " + old.SpellID + " this is above 65535 max!");
					excludes.Add(old.Id_nb);
					continue;
				}

				if (old.SpellID1 > ushort.MaxValue)
				{
					GameServer.Instance.Logger.Error(old.Name + " has a spell ID 1 of " + old.SpellID1 + " this is above 65535 max!");
					excludes.Add(old.Id_nb);
					continue;
				}
				ItemTemplate template = new ItemTemplate();
				template.AllowedClasses = old.AllowedClasses;
				template.Bonus = (byte)old.Bonus;
				template.CanDropAsLoot = old.CanDropAsLoot;
				template.CanUseEvery = old.CanUseEvery;
				template.Charges = (byte)old.Charges;
				template.Charges1 = (byte)old.Charges1;
				template.Color = (byte)old.Color;
				template.Condition = old.Condition;
				template.Copper = old.Copper;
				template.DPS_AF = (byte)old.DPS_AF;
				template.Durability = old.Durability;
				template.Effect = (byte)old.Effect;
				template.Extension = old.Extension;
				template.Gold = old.Gold;
				template.Hand = (byte)old.Hand;
				template.IsDropable = old.IsDropable;
				template.IsPickable = old.IsPickable;
				template.IsTradable = old.IsTradable;
				template.Item_Type = (byte)old.Item_Type;
				template.Level = (byte)old.Level;
				template.MaxCharges = (byte)old.MaxCharges;
				template.MaxCharges1 = (byte)old.MaxCharges1;
				template.MaxCondition = old.MaxCondition;
				template.MaxCount = (byte)old.MaxCount;
				template.MaxDurability = old.MaxDurability;
				template.Model = (ushort)old.Model;
				template.Name = old.Name;
				template.Object_Type = (byte)old.Object_Type;
				template.PackSize = (byte)old.PackSize;
				template.Platinum = old.Platinum;
				template.ProcSpellID = (ushort)old.ProcSpellID;
				template.ProcSpellID1 = (ushort)old.ProcSpellID1;
				template.Quality = (byte)old.Quality;
				template.Realm = (byte)old.Realm;
				template.Silver = old.Silver;
				template.SPD_ABS = (byte)old.SPD_ABS;
				template.SpellID = (ushort)old.SpellID;
				template.SpellID1 = (ushort)old.SpellID1;
				template.TemplateID = old.Id_nb;
				template.Type_Damage = (byte)old.Type_Damage;
				template.Weight = (byte)old.Weight;

				if (old.Bonus1Type > 0)
					template.AddBonus((byte)old.Bonus1Type, old.Bonus1);
				if (old.Bonus2Type > 0)
					template.AddBonus((byte)old.Bonus2Type, old.Bonus2);
				if (old.Bonus3Type > 0)
					template.AddBonus((byte)old.Bonus3Type, old.Bonus3);
				if (old.Bonus4Type > 0)
					template.AddBonus((byte)old.Bonus4Type, old.Bonus4);
				if (old.Bonus5Type > 0)
					template.AddBonus((byte)old.Bonus5Type, old.Bonus5);
				if (old.Bonus6Type > 0)
					template.AddBonus((byte)old.Bonus6Type, old.Bonus6);
				if (old.Bonus7Type > 0)
					template.AddBonus((byte)old.Bonus7Type, old.Bonus7);
				if (old.Bonus8Type > 0)
					template.AddBonus((byte)old.Bonus8Type, old.Bonus8);
				if (old.Bonus9Type > 0)
					template.AddBonus((byte)old.Bonus9Type, old.Bonus9);
				if (old.Bonus10Type > 0)
					template.AddBonus((byte)old.Bonus10Type, old.Bonus10);
				if (old.ExtraBonusType > 0)
					template.AddBonus((byte)old.ExtraBonusType, old.ExtraBonus);

				GameServer.Database.AddNewObject(template);
				foreach (ItemBonus bonus in template.MagicalBonuses)
					GameServer.Database.AddNewObject(bonus);

				GameServer.Database.DeleteObject(old);
				count++;
			}

			OldInventoryItem[] items = (OldInventoryItem[])GameServer.Database.SelectAllObjects(typeof(OldInventoryItem));
			total = items.Length;
			count = 0;
			GameServer.Instance.Logger.Error("Beginning convert of " + total + " inventory items!");
			convertStarted = DateTime.Now;
			foreach (OldInventoryItem old in items)
			{
				if (excludes.Contains(old.Id_nb))
					continue;
				if (old.Id_nb == "Free")
				{
					GameServer.Database.DeleteObject(old);
					continue;
				}
				//does a suitable template exist
				ItemTemplate template = (ItemTemplate)GameServer.Database.SelectObject(typeof(ItemTemplate), "`Name` = '" + GameServer.Database.Escape(old.Name) + "'");
				if (template == null)
				{

					if (old.ProcSpellID > ushort.MaxValue)
					{
						GameServer.Instance.Logger.Error(old.Name + " has a proc spell ID of " + old.ProcSpellID + " this is above 65535 max!");
						continue;
					}

					if (old.ProcSpellID1 > ushort.MaxValue)
					{
						GameServer.Instance.Logger.Error(old.Name + " has a proc spell 1 ID of " + old.ProcSpellID1 + " this is above 65535 max!");
						continue;
					}


					if (old.SpellID > ushort.MaxValue)
					{
						GameServer.Instance.Logger.Error(old.Name + " has a spell ID of " + old.SpellID + " this is above 65535 max!");
						continue;
					}
					template = new ItemTemplate();
					template.Disposable = true;
					template.AllowedClasses = old.AllowedClasses;
					template.Bonus = (byte)old.Bonus;
					template.CanDropAsLoot = old.CanDropAsLoot;
					template.CanUseEvery = old.CanUseEvery;
					template.Charges = (byte)old.Charges;
					template.Charges1 = (byte)old.Charges1;
					template.Color = (byte)old.Color;
					template.Condition = old.Condition;
					template.Copper = old.Copper;
					template.DPS_AF = (byte)old.DPS_AF;
					template.Durability = old.Durability;
					template.Effect = (byte)old.Effect;
					template.Extension = old.Extension;
					template.Gold = old.Gold;
					template.Hand = (byte)old.Hand;
					template.IsDropable = old.IsDropable;
					template.IsPickable = old.IsPickable;
					template.IsTradable = old.IsTradable;
					template.Item_Type = (byte)old.Item_Type;
					template.Level = (byte)old.Level;
					template.MaxCharges = (byte)old.MaxCharges;
					template.MaxCharges1 = (byte)old.MaxCharges1;
					template.MaxCondition = old.MaxCondition;
					template.MaxCount = (byte)old.MaxCount;
					template.MaxDurability = old.MaxDurability;
					template.Model = (ushort)old.Model;
					template.Name = old.Name;
					template.Object_Type = (byte)old.Object_Type;
					template.PackSize = (byte)old.PackSize;
					template.Platinum = old.Platinum;
					template.ProcSpellID = (ushort)old.ProcSpellID;
					template.ProcSpellID1 = (ushort)old.ProcSpellID1;
					template.Quality = (byte)old.Quality;
					template.Realm = (byte)old.Realm;
					template.Silver = old.Silver;
					template.SPD_ABS = (byte)old.SPD_ABS;
					template.SpellID = (ushort)old.SpellID;
					template.SpellID1 = (ushort)old.SpellID1;
					template.TemplateID = DOL.Database.UniqueID.IdGenerator.generateId();
					template.Type_Damage = (byte)old.Type_Damage;
					template.Weight = (byte)old.Weight;
					template.CanDropAsLoot = false;

					if (old.Bonus1Type > 0)
						template.AddBonus((byte)old.Bonus1Type, old.Bonus1);
					if (old.Bonus2Type > 0)
						template.AddBonus((byte)old.Bonus2Type, old.Bonus2);
					if (old.Bonus3Type > 0)
						template.AddBonus((byte)old.Bonus3Type, old.Bonus3);
					if (old.Bonus4Type > 0)
						template.AddBonus((byte)old.Bonus4Type, old.Bonus4);
					if (old.Bonus5Type > 0)
						template.AddBonus((byte)old.Bonus5Type, old.Bonus5);
					if (old.Bonus6Type > 0)
						template.AddBonus((byte)old.Bonus6Type, old.Bonus6);
					if (old.Bonus7Type > 0)
						template.AddBonus((byte)old.Bonus7Type, old.Bonus7);
					if (old.Bonus8Type > 0)
						template.AddBonus((byte)old.Bonus8Type, old.Bonus8);
					if (old.Bonus9Type > 0)
						template.AddBonus((byte)old.Bonus9Type, old.Bonus9);
					if (old.Bonus10Type > 0)
						template.AddBonus((byte)old.Bonus10Type, old.Bonus10);
					if (old.ExtraBonusType > 0)
						template.AddBonus((byte)old.ExtraBonusType, old.ExtraBonus);

					GameServer.Database.AddNewObject(template);
					foreach (ItemBonus bonus in template.MagicalBonuses)
						GameServer.Database.AddNewObject(bonus);
				}


				InventoryItem item = new InventoryItem(template);
				item.SlotPosition = old.SlotPosition;
				item.OwnerID = old.OwnerID;
				item.Count = (byte)old.Count;
				item.SellPrice = old.SellPrice;
				item.Description = old.CrafterName;
				item.Cooldown = old.Cooldown;
				item.Experience = old.Experience;
				item.Quality = (byte)old.Quality;
				item.Bonus = (byte)old.Bonus;
				item.Condition = old.Condition;
				item.Durability = old.Durability;
				item.Emblem = old.Emblem;
				item.Color = (byte)old.Color;
				item.Effect = (byte)old.Effect;
				item.Extension = old.Extension;
				item.Charges = (byte)old.Charges;
				item.Charges1 = (byte)old.Charges1;
				item.MaxCharges = (byte)old.MaxCharges;

				GameServer.Database.AddNewObject(item);
				GameServer.Database.DeleteObject(old);

				count++;
			}

		}

		public static int CallBack(object state)
		{
			if (!convertThread.IsAlive)
				return 0;

			if (total == 0 || count == 0)
				return 5000;

			TimeSpan elapsed = DateTime.Now.Subtract(convertStarted);
			int totalmin = (total / count) * elapsed.Minutes;

			GameServer.Instance.Logger.Info("Converted " + count + " items of " + total + " in " + (double)elapsed.TotalMinutes + " minutes completion estimated in " + (totalmin - elapsed.Minutes) + " minutes!");

			return 5000;
		}

		private static List<ushort> usedIDs = new List<ushort>();

		private static void ConvertSpell(OldDBSpell spell)
		{
			//record old id
			int oldID = spell.SpellID;
			//calculate new id
			ushort newID = 0;
			for (ushort i = 30000; i < ushort.MaxValue; i++)
			{
				/*
				 * we can't use this because we are loading spells from a different table
				if (SkillBase.GetSpellByID(i) != null)
					continue;
				 */
				if (m_spellIDs.Contains(i))
					continue;
				if (usedIDs.Contains(i))
					continue;

				newID = i;
				usedIDs.Add(i);
				break;
			}
			if (newID == 0)
			{
				GameServer.Instance.Logger.Error("Cannot convert spell " + oldID + " no new id can be found!");
				return;
			}
			spell.SpellID = newID;
			GameServer.Database.SaveObject(spell);
			//update linexspell
			OldDBLineXSpell[] oldlinexs = (OldDBLineXSpell[])GameServer.Database.SelectObjects(typeof(OldDBLineXSpell), "`SpellID` = '" + oldID + "'");
			foreach (OldDBLineXSpell oldlinex in oldlinexs)
			{
				oldlinex.SpellID = newID;
				GameServer.Database.SaveObject(oldlinex);
			}
			//update stylexspell
			OldDBStyleXSpell[] oldstylexs = (OldDBStyleXSpell[])GameServer.Database.SelectObjects(typeof(OldDBStyleXSpell), "`SpellID` = '" + oldID + "'");
			foreach (OldDBStyleXSpell oldstylex in oldstylexs)
			{
				oldstylex.SpellID = newID;
				GameServer.Database.SaveObject(oldstylex);
			}
			//update itembonus
			ItemBonus[] bonuses = (ItemBonus[])GameServer.Database.SelectObjects(typeof(ItemBonus), "`BonusType` >= 15 AND `BonusAmount` = '" + oldID + "'");
			foreach (ItemBonus bonus in bonuses)
			{
				bonus.BonusAmount = newID;
				GameServer.Database.SaveObject(bonus);
			}

			//update old itemtemplate
			OldItemTemplate[] templates = (OldItemTemplate[])GameServer.Database.SelectObjects(typeof(OldItemTemplate), "`SpellID` = '" + oldID + "' OR `SpellID1` = '" + oldID + "' OR `ProcSpellID` = '" + oldID + "' or `ProcSpellID1` = '" + oldID + "'");
			foreach (OldItemTemplate template in templates)
			{
				if (template.SpellID == oldID)
					template.SpellID = newID;
				if (template.SpellID1 == oldID)
					template.SpellID1 = newID;
				if (template.ProcSpellID == oldID)
					template.ProcSpellID = newID;
				if (template.ProcSpellID1 == oldID)
					template.ProcSpellID1 = newID;
				GameServer.Database.SaveObject(template);
			}
			//update old inventoryitem
			OldInventoryItem[] items = (OldInventoryItem[])GameServer.Database.SelectObjects(typeof(OldInventoryItem), "`SpellID` = '" + oldID + "' OR `SpellID1` = '" + oldID + "' OR `ProcSpellID` = '" + oldID + "' or `ProcSpellID1` = '" + oldID + "'");
			foreach (OldInventoryItem item in items)
			{
				if (item.SpellID == oldID)
					item.SpellID = newID;
				if (item.SpellID1 == oldID)
					item.SpellID1 = newID;
				if (item.ProcSpellID == oldID)
					item.ProcSpellID = newID;
				if (item.ProcSpellID1 == oldID)
					item.ProcSpellID1 = newID;
				GameServer.Database.SaveObject(item);
			}
			GameServer.Instance.Logger.Info("Spell " + oldID + " is now " + newID);
		}
	}
}

namespace DOL.Database
{
	[DataTable(TableName = "OldItemTemplate")]
	public class OldItemTemplate : DataObject
	{
		protected string m_id_nb;
		protected string m_name;
		protected int m_level;
		protected int m_durability;
		protected int m_maxdurability;
		protected int m_condition;
		protected int m_maxcondition;
		protected int m_quality;
		protected int m_dps_af;
		protected int m_spd_abs;
		protected int m_hand;
		protected int m_type_damage;
		protected int m_object_type;
		protected int m_item_type;
		protected int m_color;
		protected int m_emblem;
		protected int m_effect;
		protected int m_weight;
		protected int m_model;
		protected byte m_extension;
		protected int m_bonus;
		protected int m_bonus1;
		protected int m_bonus2;
		protected int m_bonus3;
		protected int m_bonus4;
		protected int m_bonus5;
		protected int m_bonus6;
		protected int m_bonus7;
		protected int m_bonus8;
		protected int m_bonus9;
		protected int m_bonus10;
		protected int m_extrabonus;
		protected int m_bonusType;
		protected int m_bonus1Type;
		protected int m_bonus2Type;
		protected int m_bonus3Type;
		protected int m_bonus4Type;
		protected int m_bonus5Type;
		protected int m_bonus6Type;
		protected int m_bonus7Type;
		protected int m_bonus8Type;
		protected int m_bonus9Type;
		protected int m_bonus10Type;
		protected int m_extrabonusType;
		protected short m_platinum;
		protected short m_gold;
		protected byte m_silver;
		protected byte m_copper;
		protected bool m_isDropable;
		protected bool m_isPickable;
		protected bool m_isTradable;
		protected bool m_canDropAsLoot;
		protected int m_maxCount;
		protected int m_packSize;
		protected int m_spellID;
		protected int m_procSpellID;
		protected int m_maxCharges;
		protected int m_charges;
		protected int m_spellID1;
		protected int m_procSpellID1;
		protected int m_charges1;
		protected int m_maxCharges1;
		protected int m_poisonSpellID;
		protected int m_poisonMaxCharges;
		protected int m_poisonCharges;
		protected int m_realm;
		private string m_allowedClasses = "";
		private int m_canUseEvery;
		static bool m_autoSave;

		public OldItemTemplate()
		{
			m_id_nb = "XXX_useless_junk_XXX";
			m_name = "Some usless junk";
			m_level = 0;
			m_durability = 1;
			m_maxdurability = 1;
			m_condition = 1;
			m_maxcondition = 1;
			m_quality = 1;
			m_dps_af = 0;
			m_spd_abs = 0;
			m_hand = 0;
			m_type_damage = 0;
			m_object_type = 0;
			m_item_type = 0;
			m_color = 0;
			m_emblem = 0;
			m_effect = 0;
			m_weight = 0;
			m_model = 488; //bag
			m_extension = 0;
			m_bonus = 0;
			m_bonus1 = 0;
			m_bonus2 = 0;
			m_bonus3 = 0;
			m_bonus4 = 0;
			m_bonus5 = 0;
			m_bonus6 = 0;
			m_bonus7 = 0;
			m_bonus8 = 0;
			m_bonus9 = 0;
			m_bonus10 = 0;
			m_extrabonus = 0;
			m_bonusType = 0;
			m_bonus1Type = 0;
			m_bonus2Type = 0;
			m_bonus3Type = 0;
			m_bonus4Type = 0;
			m_bonus5Type = 0;
			m_bonus6Type = 0;
			m_bonus7Type = 0;
			m_bonus8Type = 0;
			m_bonus9Type = 0;
			m_bonus10Type = 0;
			m_extrabonusType = 0;
			m_platinum = 0;
			m_gold = 0;
			m_silver = 0;
			m_copper = 0;
			m_isDropable = true;
			m_isPickable = true;
			m_isTradable = true;
			m_canDropAsLoot = true;
			m_maxCount = 1;
			m_packSize = 1;
			m_charges = 0;
			m_maxCharges = 0;
			m_spellID = 0;//when no spell link to item
			m_spellID1 = 0;
			m_procSpellID = 0;
			m_procSpellID1 = 0;
			m_charges1 = 0;
			m_maxCharges1 = 0;
			m_poisonCharges = 0;
			m_poisonMaxCharges = 0;
			m_poisonSpellID = 0;
			m_realm = 0;
			m_autoSave = false;
		}

		[PrimaryKey]
		public virtual string Id_nb
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				Dirty = true;
				m_id_nb = value;
			}
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Durability
		{
			get
			{
				return m_durability;
			}
			set
			{
				Dirty = true;
				m_durability = value;
			}
		}
		public byte DurabilityPercent
		{
			get
			{
				return (byte)((m_maxdurability > 0) ? m_durability * 100 / m_maxdurability : 0);
			}
		}

		[DataElement(AllowDbNull = true)]
		public int MaxDurability
		{
			get
			{
				return m_maxdurability;
			}
			set
			{
				Dirty = true;
				m_maxdurability = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Condition
		{
			get
			{
				return m_condition;
			}
			set
			{
				Dirty = true;
				m_condition = value;
			}
		}
		public byte ConditionPercent
		{
			get
			{
				return (byte)Math.Round((m_maxcondition > 0) ? (double)m_condition / m_maxcondition * 100 : 0);
			}
		}
		[DataElement(AllowDbNull = true)]
		public int MaxCondition
		{
			get
			{
				return m_maxcondition;
			}
			set
			{
				Dirty = true;
				m_maxcondition = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Quality
		{
			get
			{
				return m_quality;
			}
			set
			{
				Dirty = true;
				m_quality = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int DPS_AF
		{
			get
			{
				return m_dps_af;
			}
			set
			{
				Dirty = true;
				m_dps_af = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int SPD_ABS
		{
			get
			{
				return m_spd_abs;
			}
			set
			{
				Dirty = true;
				m_spd_abs = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Hand
		{
			get
			{
				return m_hand;
			}
			set
			{
				Dirty = true;
				m_hand = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Type_Damage
		{
			get
			{
				return m_type_damage;
			}
			set
			{
				Dirty = true;
				m_type_damage = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Object_Type
		{
			get
			{
				return m_object_type;
			}
			set
			{
				Dirty = true;
				m_object_type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Item_Type
		{
			get
			{
				return m_item_type;
			}
			set
			{
				Dirty = true;
				m_item_type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Color
		{
			get
			{
				return m_color;
			}
			set
			{
				Dirty = true;
				m_color = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				Dirty = true;
				m_emblem = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Effect
		{
			get
			{
				return m_effect;
			}
			set
			{
				Dirty = true;
				m_effect = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Weight
		{
			get
			{
				return m_weight;
			}
			set
			{
				Dirty = true;
				m_weight = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Model
		{
			get
			{
				return m_model;
			}
			set
			{
				Dirty = true;
				m_model = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Extension
		{
			get
			{
				return m_extension;
			}
			set
			{
				Dirty = true;
				m_extension = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Bonus
		{
			get
			{
				return m_bonus;
			}
			set
			{
				Dirty = true;
				m_bonus = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus1
		{
			get
			{
				return m_bonus1;
			}
			set
			{
				Dirty = true;
				m_bonus1 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus2
		{
			get
			{
				return m_bonus2;
			}
			set
			{
				Dirty = true;
				m_bonus2 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus3
		{
			get
			{
				return m_bonus3;
			}
			set
			{
				Dirty = true;
				m_bonus3 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus4
		{
			get
			{
				return m_bonus4;
			}
			set
			{
				Dirty = true;
				m_bonus4 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus5
		{
			get
			{
				return m_bonus5;
			}
			set
			{
				Dirty = true;
				m_bonus5 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus6
		{
			get
			{
				return m_bonus6;
			}
			set
			{
				Dirty = true;
				m_bonus6 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus7
		{
			get
			{
				return m_bonus7;
			}
			set
			{
				Dirty = true;
				m_bonus7 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus8
		{
			get
			{
				return m_bonus8;
			}
			set
			{
				Dirty = true;
				m_bonus8 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus9
		{
			get
			{
				return m_bonus9;
			}
			set
			{
				Dirty = true;
				m_bonus9 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus10
		{
			get
			{
				return m_bonus10;
			}
			set
			{
				Dirty = true;
				m_bonus10 = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int ExtraBonus
		{
			get
			{
				return m_extrabonus;
			}
			set
			{
				Dirty = true;
				m_extrabonus = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus1Type
		{
			get
			{
				return m_bonus1Type;
			}
			set
			{
				Dirty = true;
				m_bonus1Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus2Type
		{
			get
			{
				return m_bonus2Type;
			}
			set
			{
				Dirty = true;
				m_bonus2Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus3Type
		{
			get
			{
				return m_bonus3Type;
			}
			set
			{
				Dirty = true;
				m_bonus3Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus4Type
		{
			get
			{
				return m_bonus4Type;
			}
			set
			{
				Dirty = true;
				m_bonus4Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus5Type
		{
			get
			{
				return m_bonus5Type;
			}
			set
			{
				Dirty = true;
				m_bonus5Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus6Type
		{
			get
			{
				return m_bonus6Type;
			}
			set
			{
				Dirty = true;
				m_bonus6Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus7Type
		{
			get
			{
				return m_bonus7Type;
			}
			set
			{
				Dirty = true;
				m_bonus7Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus8Type
		{
			get
			{
				return m_bonus8Type;
			}
			set
			{
				Dirty = true;
				m_bonus8Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus9Type
		{
			get
			{
				return m_bonus9Type;
			}
			set
			{
				Dirty = true;
				m_bonus9Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public virtual int Bonus10Type
		{
			get
			{
				return m_bonus10Type;
			}
			set
			{
				Dirty = true;
				m_bonus10Type = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int ExtraBonusType
		{
			get
			{
				return m_extrabonusType;
			}
			set
			{
				Dirty = true;
				m_extrabonusType = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public bool IsPickable
		{
			get
			{
				return m_isPickable;
			}
			set
			{
				Dirty = true;
				m_isPickable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsDropable
		{
			get
			{
				return m_isDropable;
			}
			set
			{
				Dirty = true;
				m_isDropable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool CanDropAsLoot
		{
			get
			{
				return m_canDropAsLoot;
			}
			set
			{
				Dirty = true;
				m_canDropAsLoot = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool IsTradable
		{
			get
			{
				return m_isTradable;
			}
			set
			{
				Dirty = true;
				m_isTradable = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public short Platinum
		{
			get
			{
				return m_platinum;
			}
			set
			{
				Dirty = true;
				m_platinum = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public short Gold
		{
			get
			{
				return m_gold;
			}
			set
			{
				Dirty = true;
				m_gold = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public byte Silver
		{
			get
			{
				return m_silver;
			}
			set
			{
				Dirty = true;
				m_silver = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Copper
		{
			get
			{
				return m_copper;
			}
			set
			{
				Dirty = true;
				m_copper = value;
			}
		}

		/// <summary>
		/// Max amount allowed in one stack
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int MaxCount
		{
			get { return m_maxCount; }
			set
			{
				Dirty = true;
				m_maxCount = value;
			}
		}

		/// <summary>
		/// Amount of items sold at once
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int PackSize
		{
			get { return m_packSize; }
			set
			{
				Dirty = true;
				m_packSize = value;
			}
		}

		/// <summary>
		/// Charge of item when he have some charge of a spell
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Charges
		{
			get { return m_charges; }
			set
			{
				Dirty = true;
				m_charges = value;
			}
		}

		/// <summary>
		/// Max charge of item when he have some charge of a spell
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int MaxCharges
		{
			get { return m_maxCharges; }
			set
			{
				Dirty = true;
				m_maxCharges = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Charges1
		{
			get { return m_charges1; }
			set
			{
				Dirty = true;
				m_charges1 = value;
			}
		}

		/// <summary>
		/// Max charge of item when he have some charge of a spell
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int MaxCharges1
		{
			get { return m_maxCharges1; }
			set
			{
				Dirty = true;
				m_maxCharges1 = value;
			}
		}

		/// <summary>
		/// Spell id for items with charge
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual int SpellID
		{
			get { return m_spellID; }
			set
			{
				Dirty = true;
				m_spellID = value;
			}
		}

		/// <summary>
		/// Spell id for items with charge
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual int SpellID1
		{
			get { return m_spellID1; }
			set
			{
				Dirty = true;
				m_spellID1 = value;
			}
		}

		/// <summary>
		/// ProcSpell id for items
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public virtual int ProcSpellID
		{
			get { return m_procSpellID; }
			set
			{
				Dirty = true;
				m_procSpellID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public virtual int ProcSpellID1
		{
			get { return m_procSpellID1; }
			set
			{
				Dirty = true;
				m_procSpellID1 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int PoisonSpellID
		{
			get { return m_poisonSpellID; }
			set
			{
				Dirty = true;
				m_poisonSpellID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int PoisonMaxCharges
		{
			get { return m_poisonMaxCharges; }
			set
			{
				Dirty = true;
				m_poisonMaxCharges = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int PoisonCharges
		{
			get { return m_poisonCharges; }
			set
			{
				Dirty = true;
				m_poisonCharges = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Realm
		{
			get { return m_realm; }
			set
			{
				m_realm = value;
				Dirty = true;
			}
		}

		/// <summary>
		/// the serialized allowed classes of item
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string AllowedClasses
		{
			get { return m_allowedClasses; }
			set
			{
				m_allowedClasses = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int CanUseEvery
		{
			get { return m_canUseEvery; }
			set
			{
				m_canUseEvery = value;
				Dirty = true;
			}
		}
	}
}

namespace DOL.Database
{
	/// <summary>
	/// The InventoryItem table holds all values from the
	/// ItemTemplate table and also some more values that
	/// are neccessary to store the inventory position
	/// </summary>
	[DataTable(TableName = "OldInventoryItem")]
	public class OldInventoryItem : OldItemTemplate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected string m_ownerID;
		protected int m_slot_pos;
		protected string craftername;
		protected int m_canUseAgainIn;
        protected long item_exp;
		private int m_cooldown;
		private DateTime m_lastUsedDateTime;

		/// <summary>
		/// The count of items (for stack!)
		/// </summary>
		protected int m_count;
		static bool m_autoSave;
        protected string m_internalID;
		protected int m_sellPrice;

		public OldInventoryItem()
			: base()
		{
			m_autoSave = false;
			m_id_nb = "default";
			m_count = 1;
            m_internalID = this.ObjectId;
			m_sellPrice = 0;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public override string Id_nb
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				Dirty = true;
				m_id_nb = value;
			}
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public string OwnerID
		{
			get
			{
				return m_ownerID;
			}
			set
			{
				Dirty = true;
				m_ownerID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Count
		{
			get
			{
				return m_count;
			}
			set
			{
				Dirty = true;
				m_count = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SlotPosition
		{
			get
			{
				return m_slot_pos;
			}
			set
			{
				Dirty = true;
				m_slot_pos = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SellPrice
		{
			get
			{
				return m_sellPrice;
			}
			set
			{
				Dirty = true;
				m_sellPrice = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string CrafterName
		{
			get
			{
				return craftername;
			}
			set
			{
				Dirty = true;
				craftername = value;
			}
		}

		/// <summary>
		/// Internal use only!
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Cooldown
		{
			get { return CanUseAgainIn; }
			set { m_cooldown = value; }
		}

		/// <summary>
		/// Internal use only!
		/// </summary>
		public void SetCooldown()
		{
			CanUseAgainIn = m_cooldown;
		}

		/// <summary>
		/// When this item can be used again (in seconds).
		/// </summary>
		public int CanUseAgainIn
		{
			get 
			{
				try
				{
					TimeSpan elapsed = DateTime.Now.Subtract(m_lastUsedDateTime);
					TimeSpan reuse = new TimeSpan(0, 0, CanUseEvery);
					return (reuse.CompareTo(elapsed) < 0) 
						? 0 
						: CanUseEvery - elapsed.Seconds - 60 * elapsed.Minutes - 3600 * elapsed.Hours;
				}
				catch (ArgumentOutOfRangeException)
				{
					return 0;
				}
			}
			set
			{
				m_lastUsedDateTime = DateTime.Now.AddSeconds(value - CanUseEvery);
				Dirty = true;
			}
		}

        [DataElement(AllowDbNull = false)]
        public virtual long Experience
        {
            get { return item_exp; }
            set
            {
                Dirty = true;
                item_exp = value;
            }
        }

		/// <summary>
		/// Whether to save this object or not.
		/// </summary>
		public override bool Dirty
		{
			get
			{
				// Items with reuse timers will ALWAYS be saved.

				return (base.Dirty || CanUseEvery > 0);
			}
			set
			{
				base.Dirty = value;
			}
		}
	}
}

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName = "OldSpell")]
	public class OldDBSpell : DataObject
	{
		protected string m_id_unique;
		protected int m_spellid;
		protected int m_effectid;
		protected int m_icon;
		protected string m_name;
		protected string m_description;
		protected string m_target = "";

		protected string m_spelltype = "";
		protected int m_range = 0;
		protected int m_radius = 0;
		protected double m_value = 0;
		protected double m_damage = 0;
		protected int m_damageType = 0;
		protected int m_concentration = 0;
		protected int m_duration = 0;
		protected int m_pulse = 0;
		protected int m_frequency = 0;
		protected int m_pulse_power = 0;
		protected int m_power = 0;
		protected double m_casttime = 0;
		protected int m_recastdelay = 0;
		protected int m_reshealth = 1;
		protected int m_resmana = 0;
		protected int m_lifedrain_return = 0;
		protected int m_amnesia_chance = 0;
		protected string m_message1 = "";
		protected string m_message2 = "";
		protected string m_message3 = "";
		protected string m_message4 = "";
		protected int m_instrumentRequirement;
		protected int m_spellGroup;
		protected int m_effectGroup;
		protected int m_subSpellID = 0;
		protected bool m_moveCast = false;
		protected bool m_uninterruptible = false;
		protected int m_healthPenalty = 0;
		protected bool m_isfocus = false;
        	protected int m_sharedtimergroup; 
        
		// warlock
		protected bool m_isprimary;
		protected bool m_issecondary;
		protected bool m_allowbolt;
		static bool m_autoSave;

		public OldDBSpell()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull = false, Unique = true)]
		public int SpellID
		{
			get
			{
				return m_spellid;
			}
			set
			{
				Dirty = true;
				m_spellid = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int ClientEffect
		{
			get
			{
				return m_effectid;
			}
			set
			{
				Dirty = true;
				m_effectid = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Icon
		{
			get
			{
				return m_icon;
			}
			set
			{
				Dirty = true;
				m_icon = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				Dirty = true;
				m_description = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Target
		{
			get
			{
				return m_target;
			}
			set
			{
				Dirty = true;
				m_target = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Range
		{
			get
			{
				return m_range;
			}
			set
			{
				Dirty = true;
				m_range = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Power
		{
			get
			{
				return m_power;
			}
			set
			{
				Dirty = true;
				m_power = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public double CastTime
		{
			get
			{
				return m_casttime;
			}
			set
			{
				Dirty = true;
				m_casttime = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public double Damage
		{
			get
			{
				return m_damage;
			}
			set
			{
				Dirty = true;
				m_damage = value;
			}
		}


		[DataElement(AllowDbNull = true)]
		public int DamageType
		{
			get
			{
				return m_damageType;
			}
			set
			{
				Dirty = true;
				m_damageType = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Type
		{
			get
			{
				return m_spelltype;
			}
			set
			{
				Dirty = true;
				m_spelltype = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Duration
		{
			get
			{
				return m_duration;
			}
			set
			{
				Dirty = true;
				m_duration = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Frequency
		{
			get
			{
				return m_frequency;
			}
			set
			{
				Dirty = true;
				m_frequency = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Pulse
		{
			get
			{
				return m_pulse;
			}
			set
			{
				Dirty = true;
				m_pulse = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int PulsePower
		{
			get
			{
				return m_pulse_power;
			}
			set
			{
				Dirty = true;
				m_pulse_power = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Radius
		{
			get
			{
				return m_radius;
			}
			set
			{
				Dirty = true;
				m_radius = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int RecastDelay
		{
			get
			{
				return m_recastdelay;
			}
			set
			{
				Dirty = true;
				m_recastdelay = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int ResurrectHealth
		{
			get
			{
				return m_reshealth;
			}
			set
			{
				Dirty = true;
				m_reshealth = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int ResurrectMana
		{
			get
			{
				return m_resmana;
			}
			set
			{
				Dirty = true;
				m_resmana = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public double Value
		{
			get
			{
				return m_value;
			}
			set
			{
				Dirty = true;
				m_value = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Concentration
		{
			get
			{
				return m_concentration;
			}
			set
			{
				Dirty = true;
				m_concentration = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int LifeDrainReturn
		{
			get
			{
				return m_lifedrain_return;
			}
			set
			{
				Dirty = true;
				m_lifedrain_return = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int AmnesiaChance
		{
			get
			{
				return m_amnesia_chance;
			}
			set
			{
				Dirty = true;
				m_amnesia_chance = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Message1
		{
			get { return m_message1; }
			set
			{
				Dirty = true;
				m_message1 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Message2
		{
			get { return m_message2; }
			set
			{
				Dirty = true;
				m_message2 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Message3
		{
			get { return m_message3; }
			set
			{
				Dirty = true;
				m_message3 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Message4
		{
			get { return m_message4; }
			set
			{
				Dirty = true;
				m_message4 = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int InstrumentRequirement
		{
			get { return m_instrumentRequirement; }
			set
			{
				Dirty = true;
				m_instrumentRequirement = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SpellGroup
		{
			get
			{
				return m_spellGroup;
			}
			set
			{
				Dirty = true;
				m_spellGroup = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int EffectGroup
		{
			get
			{
				return m_effectGroup;
			}
			set
			{
				Dirty = true;
				m_effectGroup = value;
			}
		}

		//Multiple spells
		[DataElement(AllowDbNull = true)]
		public int SubSpellID
		{
			get
			{
				return m_subSpellID;
			}
			set
			{
				Dirty = true;
				m_subSpellID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool MoveCast
		{
			get { return m_moveCast; }
			set
			{
				Dirty = true;
				m_moveCast = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool Uninterruptible
		{
			get { return m_uninterruptible; }
			set
			{
				Dirty = true;
				m_uninterruptible = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int HealthPenalty
		{
			get { return m_healthPenalty; }
			set
			{
				Dirty = true;
				m_healthPenalty = value;
			}
		}
		
		[DataElement(AllowDbNull = true)]
		public bool IsFocus
		{
			get { return m_isfocus; }
			set
			{
				Dirty = true;
				m_isfocus = value;
			}
		}
        [DataElement(AllowDbNull = true)]
        public int SharedTimerGroup
        {
            get
            {
                return m_sharedtimergroup;
            }
            set
            {
                Dirty = true;
                m_sharedtimergroup = value;
            }
        }

		#region warlock
		[DataElement(AllowDbNull = true)]
		public bool IsPrimary
		{
			get
			{
				return (bool)m_isprimary;
			}
			set
			{
				Dirty = true;
				m_isprimary = (bool)value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public bool IsSecondary
		{
			get
			{
				return (bool)m_issecondary;
			}
			set
			{
				Dirty = true;
				m_issecondary = (bool)value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public bool AllowBolt
		{
			get
			{
				return (bool)m_allowbolt;
			}
			set
			{
				Dirty = true;
				m_allowbolt = (bool)value;
			}
		}
		#endregion
	}

	[DataTable(TableName = "OldLineXSpell")]
	public class OldDBLineXSpell : DataObject
	{
		protected string m_line_name;
		protected int m_spellid;
		protected int m_level;

		static bool m_autoSave;

		public OldDBLineXSpell()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public string LineName
		{
			get
			{
				return m_line_name;
			}
			set
			{
				Dirty = true;
				m_line_name = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int SpellID
		{
			get
			{
				return m_spellid;
			}
			set
			{
				Dirty = true;
				m_spellid = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}
	}

	[DataTable(TableName = "OldStyleXSpell")]
	public class OldDBStyleXSpell : DataObject
	{
		static bool m_autoSave = false;
		protected int m_SpellID;
		protected int m_ClassID;
		protected int m_StyleID;
		protected int m_Chance;


		/// <summary>
		/// The Constructor
		/// </summary>
		public OldDBStyleXSpell()
			: base()
		{
		}

		/// <summary>
		/// The Spell ID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int SpellID
		{
			get { return m_SpellID; }
			set { m_SpellID = value; Dirty = true; }
		}

		/// <summary>
		/// The ClassID, required for style subsitute procs (0 is not a substitute style)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int ClassID
		{
			get { return m_ClassID; }
			set { m_ClassID = value; Dirty = true; }
		}

		/// <summary>
		/// The StyleID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int StyleID
		{
			get { return m_StyleID; }
			set { m_StyleID = value; Dirty = true; }
		}

		/// <summary>
		/// The Chance to add to the styleeffect list
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Chance
		{
			get { return m_Chance; }
			set { m_Chance = value; Dirty = true; }
		}

		/// <summary>
		/// Auto save is not needed the value aren't changed in game
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}
	}
}

