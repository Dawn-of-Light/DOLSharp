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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	public enum eItemBonusType
	{
		Strength = 1,
		Dexterity = 2,
		Consitution = 3,
		Quickness = 4,
		Intelligence = 5,
		Piety = 6,
		Empathy = 7,
		Charisma = 8,
		Mana = 9,
		Health = 10,
		Resist_Body = 11,
		Resist_Cold = 12,
		Resist_Crush = 13,
		Resist_Energy = 14,
		Resist_Heat = 15,
		Resist_Matter = 16,
		Resist_Slash = 17,
		Resist_Spirit = 18,
		Resist_Thrust = 19,
		Skill_Two_Handed = 20,
		Skill_Body = 21,
		Skill_Chants = 22,
		Skill_Critical_strike = 23,
		Skill_Cross_Bows = 24,
		Skill_Crushing = 25,
		Skill_Death_Servant = 26,
		Skill_DeathSight = 27,
		Skill_Dual_Wield = 28,
		Skill_Earth = 29,
		Skill_Enhancement = 30,
		Skill_Envenan = 31,
		Skill_Fire = 32,
		Skill_Flexible_Weapon = 33,
		Skill_Ice = 34,
		Skill_Instruments = 35,
		Skill_Long_bows = 36,
		Skill_Matter = 37,
		Skill_Mind = 38,
		Skill_Pain_working = 39,
		Skill_Parry = 40,
		Skill_Polearms = 41,
		Skill_Rejuvenation = 42,
		Skill_Shields = 43,
		Skill_Slashing = 44,
		Skill_Smiting = 45,
		Skill_SoulRending = 46,
		Skill_Spirit = 47,
		Skill_Staff = 48,
		Skill_Stealth = 49,
		Skill_Thrusting = 50,
		Skill_Wind = 51,
		Focus = 52,//add all focus
	};

	/// <summary>
	/// The InventoryItem table holds all values from the
	/// ItemTemplate table and also some more values that
	/// are neccessary to store the inventory position
	/// </summary>
	[DataTable(TableName="InventoryItem")]
	public class InventoryItem : ItemTemplate
	{
		protected string	m_ownerID;
		protected int		m_slot_pos;
		protected string	craftername;

		/// <summary>
		/// The count of items (for stack!)
		/// </summary>
		protected int		m_count;
		static bool			m_autoSave;

		public InventoryItem() : base()
		{
			m_autoSave=false;
			m_id_nb = "default";
			m_count = 1;
		}

		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// </summary>
		/// <param name="itemTemplate"></param>
		public InventoryItem(ItemTemplate itemTemplate) : this()
		{
			CopyFrom(itemTemplate);
		}

		/// <summary>
		/// Creates a new Inventoryitem based on the given ItemTemplate
		/// </summary>
		/// <param name="inventoryItem"></param>
		public InventoryItem(InventoryItem inventoryItem) : this()
		{
			CopyFrom(inventoryItem);
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
		
		[DataElement(AllowDbNull=false)]
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

		[DataElement(AllowDbNull=false, Index=true)]
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

		[DataElement(AllowDbNull=true)]
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

		[DataElement(AllowDbNull=true)]
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
		
		[DataElement(AllowDbNull=true)]
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

		public void CopyFrom(InventoryItem template)
		{
			OwnerID = template.OwnerID;
			Count = template.Count;
			SlotPosition = template.SlotPosition;
			CrafterName = template.CrafterName;
			CopyFrom((ItemTemplate)template);
		}

		public void CopyFrom(ItemTemplate template)
		{
			OwnerID = null;
			SlotPosition = 0;
			Bonus = template.Bonus;
			Bonus1 = template.Bonus1;
			Bonus2 = template.Bonus2;
			Bonus3 = template.Bonus3;
			Bonus4 = template.Bonus4;
			Bonus5 = template.Bonus5;
			Color = template.Color;
			Condition = template.Condition;
			DPS_AF = template.DPS_AF;
			Durability = template.Durability;
			Effect = template.Effect;
			Emblem = template.Emblem;
			ExtraBonus = template.ExtraBonus;
			Hand = template.Hand;
			Id_nb = template.Id_nb;
			IsDropable = template.IsDropable;
			IsPickable = template.IsPickable;
			MaxCount = template.MaxCount;
			PackSize = template.PackSize;
			Item_Type = template.Item_Type;
			Level = template.Level;
			MaxCondition = template.MaxCondition;
			MaxDurability = template.MaxDurability;
			MaxQuality = template.MaxQuality;
			Model = template.Model;
			Extension = template.Extension;
			Name = template.Name;
			Object_Type = template.Object_Type;
			Quality = template.Quality;
			SPD_ABS = template.SPD_ABS;
			Type_Damage = template.Type_Damage;
			Weight = template.Weight;
			Gold = template.Gold;
			Silver = template.Silver;
			Copper = template.Copper;
			Bonus1Type = template.Bonus1Type;
			Bonus2Type = template.Bonus2Type;
			Bonus3Type = template.Bonus3Type;
			Bonus4Type = template.Bonus4Type;
			Bonus5Type = template.Bonus5Type;
			ExtraBonusType = template.ExtraBonusType;
			Charges = template.Charges;
			MaxCharges = template.MaxCharges;
			SpellID = template.SpellID;
			ProcSpellID = template.ProcSpellID;
			Realm = template.Realm;
		}
	}
}