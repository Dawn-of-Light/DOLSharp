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

namespace DOL.Database
{
	[DataTable(TableName = "CharacterClass")]
	public class DBCharacterClass : DataObject
	{
		private int id;
		private int specPointMultiplier;
		private int baseHP;
		private int baseWeaponSkill;
		private string autoTrainableSkills;
		private string eligibleRaces;

		[PrimaryKey]
		public int ID
		{
			get => id; 
			set
			{
				Dirty = true;
				id = value;
			}
		}

		[DataElement]
		public int SpecPointMultiplier
		{
			get { return specPointMultiplier; }
			set
			{
				Dirty = true;
				specPointMultiplier = value;
			}
		}

		[DataElement]
		public int BaseHP
		{
			get { return baseHP; }
			set
			{
				Dirty = true;
				baseHP = value;
			}
		}

		[DataElement]
		public int BaseWeaponSkill
		{
			get { return baseWeaponSkill; }
			set
			{
				Dirty = true;
				baseWeaponSkill = value;
			}
		}

		[DataElement]
		public string AutoTrainSkills
		{
			get { return autoTrainableSkills; }
			set
			{
				Dirty = true;
				autoTrainableSkills = value;
			}
		}

		[DataElement]
		public string EligibleRaces
		{
			get { return eligibleRaces; }
			set
			{
				Dirty = true;
				eligibleRaces = value;
			}
		}
	}
}
