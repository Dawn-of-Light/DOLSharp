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
using DOL.Database.Attributes;
using log4net;

namespace DOL.Database
{
	[DataTable(TableName = "ItemUnique", PreCache = false)]
	public class ItemUnique : ItemTemplate
	{
		public ItemUnique():base()
		{
			m_id_nb = "Unique_" + UniqueID.IDGenerator.GenerateID();
			m_name = "(blank item)";
		}

		public ItemUnique(ItemTemplate template) : base()
		{
			m_id_nb = "Unique_" + UniqueID.IDGenerator.GenerateID();
			Name = template.Name;
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
			IsDropable = template.IsDropable;
			IsPickable = template.IsPickable;
			IsTradable = template.IsTradable;
			CanDropAsLoot = template.CanDropAsLoot;
			MaxCount = template.MaxCount;
			PackSize = template.PackSize;
			Item_Type = template.Item_Type;
			Level = template.Level;
			MaxCondition = template.MaxCondition;
			MaxDurability = template.MaxDurability;
			Model = template.Model;
			Extension = template.Extension;
			Object_Type = template.Object_Type;
			Quality = template.Quality;
			SPD_ABS = template.SPD_ABS;
			Type_Damage = template.Type_Damage;
			Weight = template.Weight;
			Price = template.Price;
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
			AllowedClasses = template.AllowedClasses;
			CanUseEvery = template.CanUseEvery;
			Flags = template.Flags;
			BonusLevel = template.BonusLevel;
			Description = template.Description;
			IsIndestructible = template.IsIndestructible;
			IsNotLosingDur = template.IsNotLosingDur;
		}

		public override string Id_nb
		{
			get
			{
				return m_id_nb;
			}
			set
			{
				// set once when created, never changed
			}
		}

		
		public override bool Dirty {
			get { return true; }
		}
	}
}
