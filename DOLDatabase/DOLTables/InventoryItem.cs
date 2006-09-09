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
	/// <summary>
	/// The Bonus Type ID
	/// </summary>
	public enum eItemBonusType
	{
		/// <summary>
		/// Strength
		/// </summary>
		Strength = 1,
		/// <summary>
		/// Dexterity
		/// </summary>
		Dexterity = 2,
		/// <summary>
		/// Constitution
		/// </summary>
		Constitution = 3,
		/// <summary>
		/// Quickness
		/// </summary>
		Quickness = 4,
		/// <summary>
		/// Intelligence
		/// </summary>
		Intelligence = 5,
		/// <summary>
		/// Piety
		/// </summary>
		Piety = 6,
		/// <summary>
		/// Empathy
		/// </summary>
		Empathy = 7,
		/// <summary>
		/// Charisma
		/// </summary>
		Charisma = 8,
		/// <summary>
		/// Max Mana
		/// </summary>
		Mana = 9,
		/// <summary>
		/// Max Health
		/// </summary>
		Health = 10,
		/// <summary>
		/// Body Resist
		/// </summary>
		Resist_Body = 11,
		/// <summary>
		/// Cold Resist
		/// </summary>
		Resist_Cold = 12,
		/// <summary>
		/// Crush Resist
		/// </summary>
		Resist_Crush = 13,
		/// <summary>
		/// Energy Resist
		/// </summary>
		Resist_Energy = 14,
		/// <summary>
		/// Heat Resist
		/// </summary>
		Resist_Heat = 15,
		/// <summary>
		/// Matter Resist
		/// </summary>
		Resist_Matter = 16,
		/// <summary>
		/// Slash Resist
		/// </summary>
		Resist_Slash = 17,
		/// <summary>
		/// Spirit Resist
		/// </summary>
		Resist_Spirit = 18,
		/// <summary>
		/// Thrust Resist
		/// </summary>
		Resist_Thrust = 19,
		/// <summary>
		/// Two Handed Skill
		/// </summary>
		Skill_Two_Handed = 20,
		/// <summary>
		/// Body Magic Skill
		/// </summary>
		Skill_Body = 21,
		/// <summary>
		/// Chants Skill
		/// </summary>
		Skill_Chants = 22,
		/// <summary>
		/// Critical Strike Skill
		/// </summary>
		Skill_Critical_Strike = 23,
		/// <summary>
		/// Cross Bows Skill
		/// </summary>
		Skill_Cross_Bows = 24,
		/// <summary>
		/// Crushing Skill
		/// </summary>
		Skill_Crushing = 25,
		/// <summary>
		/// Death Servant Magic Skill
		/// </summary>
		Skill_Death_Servant = 26,
		/// <summary>
		/// Death Sight Magic Skill
		/// </summary>
		Skill_DeathSight = 27,
		/// <summary>
		/// Dual Wield Skill
		/// </summary>
		Skill_Dual_Wield = 28,
		/// <summary>
		/// Earth Magic Skill
		/// </summary>
		Skill_Earth = 29,
		/// <summary>
		/// Enhancement Magic Skill
		/// </summary>
		Skill_Enhancement = 30,
		/// <summary>
		/// Envenom Skill
		/// </summary>
		Skill_Envenom = 31,
		/// <summary>
		/// Fire Magic Skill
		/// </summary>
		Skill_Fire = 32,
		/// <summary>
		/// Flexible Weapon Skill
		/// </summary>
		Skill_Flexible_Weapon = 33,
		/// <summary>
		/// Ice Magic Skill
		/// </summary>
		Skill_Ice = 34,
		/// <summary>
		/// Instruments Skill
		/// </summary>
		Skill_Instruments = 35,
		/// <summary>
		/// Longbow Skill
		/// </summary>
		Skill_Longbows = 36,
		/// <summary>
		/// Matter Magic Skill
		/// </summary>
		Skill_Matter = 37,
		/// <summary>
		/// Mind Magic Skill
		/// </summary>
		Skill_Mind = 38,
		/// <summary>
		/// Painworking Magic Skill
		/// </summary>
		Skill_Painworking = 39,
		/// <summary>
		/// Parry Skill
		/// </summary>
		Skill_Parry = 40,
		/// <summary>
		/// Polearms Skill
		/// </summary>
		Skill_Polearms = 41,
		/// <summary>
		/// Rejuvenation Skill
		/// </summary>
		Skill_Rejuvenation = 42,
		/// <summary>
		/// Shields Skill
		/// </summary>
		Skill_Shields = 43,
		/// <summary>
		/// Slashing Skill
		/// </summary>
		Skill_Slashing = 44,
		/// <summary>
		/// Smiting Skill
		/// </summary>
		Skill_Smiting = 45,
		/// <summary>
		/// Soulrending Skill
		/// </summary>
		Skill_SoulRending = 46,
		/// <summary>
		/// Spirit Magic Skill
		/// </summary>
		Skill_Spirit = 47,
		/// <summary>
		/// Staff Skill
		/// </summary>
		Skill_Staff = 48,
		/// <summary>
		/// Stealth Skill
		/// </summary>
		Skill_Stealth = 49,
		/// <summary>
		/// Thrusting Skill
		/// </summary>
		Skill_Thrusting = 50,
		/// <summary>
		/// Wind Magic Skill
		/// </summary>
		Skill_Wind = 51,
		/// <summary>
		/// Focus Magics.....
		/// </summary>
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
			Bonus6 = template.Bonus6;
			Bonus7 = template.Bonus7;
			Bonus8 = template.Bonus8;
			Bonus9 = template.Bonus9;
			Bonus10 = template.Bonus10;
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
			Platinum = template.Platinum;
			Gold = template.Gold;
			Silver = template.Silver;
			Copper = template.Copper;
			Bonus1Type = template.Bonus1Type;
			Bonus2Type = template.Bonus2Type;
			Bonus3Type = template.Bonus3Type;
			Bonus4Type = template.Bonus4Type;
			Bonus5Type = template.Bonus5Type;
			Bonus6Type = template.Bonus6Type;
			Bonus7Type = template.Bonus7Type;
			Bonus8Type = template.Bonus8Type;
			Bonus9Type = template.Bonus9Type;
			Bonus10Type = template.Bonus10Type;
			ExtraBonusType = template.ExtraBonusType;
			Charges = template.Charges;
			MaxCharges = template.MaxCharges;
			Charges1 = template.Charges1;
			MaxCharges1 = template.MaxCharges1;
			SpellID = template.SpellID;
			SpellID1 = template.SpellID1;
			ProcSpellID = template.ProcSpellID;
			ProcSpellID1 = template.ProcSpellID1;
			PoisonSpellID = template.PoisonSpellID;
			PoisonCharges = template.PoisonCharges;
			PoisonMaxCharges = template.PoisonMaxCharges;
			Realm = template.Realm;
		}
	}
}