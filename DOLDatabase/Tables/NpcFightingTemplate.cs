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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// NpcFightingTemplate describe Npc Fighting Template Behavior
	/// </summary>
	[DataTable(TableName = "npc_fightingtemplate")]
	public class NpcFightingTemplate : DataObject
	{
		private long m_npc_fightingtemplateID;
		
		/// <summary>
		/// Npc Fighting Template Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_fightingtemplateID {
			get { return m_npc_fightingtemplateID; }
			set { m_npc_fightingtemplateID = value; }
		}
		
		private string m_fightingTemplateID;
		
		/// <summary>
		/// Fighting Template ID
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string FightingTemplateID {
			get { return m_fightingTemplateID; }
			set { m_fightingTemplateID = value; Dirty = true; }
		}		

		private byte m_meleeDamageType;
		
		/// <summary>
		/// Melee Damage Type (Default 1 = crush, 2 = slash, 3 = thrust)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MeleeDamageType {
			get { return m_meleeDamageType; }
			set { m_meleeDamageType = value; Dirty = true; }
		}
		
		private byte m_blockChance;
		
		/// <summary>
		/// Block Chance (should be equiped with shield in left hand)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte BlockChance {
			get { return m_blockChance; }
			set { m_blockChance = value; Dirty = true; }
		}
		
		private byte m_leftWeaponSwingChance;
		
		/// <summary>
		/// Left swing chance (should be equiped with weapon in left hand or no weapon at all)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte LeftWeaponSwingChance {
			get { return m_leftWeaponSwingChance; }
			set { m_leftWeaponSwingChance = value; Dirty = true; }
		}
		
		private byte m_evadeChance;
		
		/// <summary>
		/// Evade Chance (doesn't need specific equipment)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte EvadeChance {
			get { return m_evadeChance; }
			set { m_evadeChance = value; Dirty = true; }
		}
		
		private byte m_parryChance;
		
		/// <summary>
		/// Parry Chance (not ranged weapon)
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte ParryChance {
			get { return m_parryChance; }
			set { m_parryChance = value; Dirty = true; }
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcFightingTemplate()
		{
			// default to crush
			m_meleeDamageType = 1;
		}
	}
}
