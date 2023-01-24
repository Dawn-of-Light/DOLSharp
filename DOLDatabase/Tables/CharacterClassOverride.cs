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
using DOL.Database.Attributes;
using DOL.Database;

namespace DOLDatabase
{
	/// <summary>
	/// Values which override hardcoded class default properties
	/// </summary>
	[DataTable(TableName = "CharacterClassOverride")]
	public class CharacterClassOverride : DataObject
	{
		// TEGS Rename CharacterClassOverride?

		private int m_classID;
		private int m_specializationMultiplier;
		private int m_baseHP;
		private int m_baseWeaponSkill;
		private int m_baseWeaponSkillRanged;
		private string m_autoTrainableSkills;
		private string m_eligibleRaces;
		private int m_maxPulsingSpells;

		[PrimaryKey]
		public int ClassID
		{
			get => m_classID; 
			set
			{
				Dirty = true;
				m_classID = value;
			}
		}

		/// <summary>
		/// Spec points gained per level
		/// </summary>
		[DataElement]
		public int SpecializationMultiplier
		{
			get { return m_specializationMultiplier; }
			set
			{
				Dirty = true;
				m_specializationMultiplier = value;
			}
		}

		/// <summary>
		/// Base hits
		/// </summary>
		[DataElement]
		public int BaseHP
		{
			get { return m_baseHP; }
			set
			{
				Dirty = true;
				m_baseHP = value;
			}
		}

		/// <summary>
		/// Base melee weapon skill
		/// </summary>
		[DataElement]
		public int BaseWeaponSkill
		{
			get { return m_baseWeaponSkill; }
			set
			{
				Dirty = true;
				m_baseWeaponSkill = value;
			}
		}

		/// <summary>
		/// Base ranged weapon skill
		/// </summary>
		[DataElement]
		public int BaseWeaponSkillRanged
		{
			get { return m_baseWeaponSkillRanged; }
			set
			{
				Dirty = true;
				m_baseWeaponSkillRanged = value;
			}
		}

		/// <summary>
		/// Skills which are auto trained
		/// </summary>
		[DataElement]
		public string AutoTrainableSkills
		{
			get { return m_autoTrainableSkills; }
			set
			{
				Dirty = true;
				m_autoTrainableSkills = value;
			}
		}

		/// <summary>
		/// Which races are eligible for this class
		/// </summary>
		[DataElement]
		public string EligibleRaces
		{
			get { return m_eligibleRaces; }
			set
			{
				Dirty = true;
				m_eligibleRaces = value;
			}
		}

		/// <summary>
		/// How many pulsing spells a class can have running at once
		/// </summary>
		[DataElement]
		public int MaxPulsingSpells
		{
			get { return m_maxPulsingSpells; }
			set
			{
				Dirty = true;
				m_maxPulsingSpells = value;
			}
		}
	}
}
