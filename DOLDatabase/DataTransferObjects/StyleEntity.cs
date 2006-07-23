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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct StyleEntity
	{
		private int m_id;
		private int m_attackResultRequirement;
		private int m_bonusToDefense;
		private int m_bonusToHit;
		private int m_enduranceCost;
		private double m_growthRate;
		private string m_keyName;
		private string m_name;
		private int m_openingRequirementType;
		private int m_openingRequirementValue;
		private int m_specialType;
		private int m_specialValue;
		private string m_specKeyName;
		private int m_specLevelRequirement;
		private string m_stealthRequirement;
		private int m_twoHandAnimation;
		private int m_weaponTypeRequirement;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int AttackResultRequirement
		{
			get { return m_attackResultRequirement; }
			set { m_attackResultRequirement = value; }
		}
		public int BonusToDefense
		{
			get { return m_bonusToDefense; }
			set { m_bonusToDefense = value; }
		}
		public int BonusToHit
		{
			get { return m_bonusToHit; }
			set { m_bonusToHit = value; }
		}
		public int EnduranceCost
		{
			get { return m_enduranceCost; }
			set { m_enduranceCost = value; }
		}
		public double GrowthRate
		{
			get { return m_growthRate; }
			set { m_growthRate = value; }
		}
		public string KeyName
		{
			get { return m_keyName; }
			set { m_keyName = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public int OpeningRequirementType
		{
			get { return m_openingRequirementType; }
			set { m_openingRequirementType = value; }
		}
		public int OpeningRequirementValue
		{
			get { return m_openingRequirementValue; }
			set { m_openingRequirementValue = value; }
		}
		public int SpecialType
		{
			get { return m_specialType; }
			set { m_specialType = value; }
		}
		public int SpecialValue
		{
			get { return m_specialValue; }
			set { m_specialValue = value; }
		}
		public string SpecKeyName
		{
			get { return m_specKeyName; }
			set { m_specKeyName = value; }
		}
		public int SpecLevelRequirement
		{
			get { return m_specLevelRequirement; }
			set { m_specLevelRequirement = value; }
		}
		public string StealthRequirement
		{
			get { return m_stealthRequirement; }
			set { m_stealthRequirement = value; }
		}
		public int TwoHandAnimation
		{
			get { return m_twoHandAnimation; }
			set { m_twoHandAnimation = value; }
		}
		public int WeaponTypeRequirement
		{
			get { return m_weaponTypeRequirement; }
			set { m_weaponTypeRequirement = value; }
		}
	}
}
