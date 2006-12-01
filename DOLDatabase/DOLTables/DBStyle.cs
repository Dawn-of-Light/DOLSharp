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
	/// TODO: add neccessary fields for styles
	/// </summary>
	[DataTable(TableName = "Style")]
	public class DBStyle : DataObject
	{
		/// <summary>
		/// The ID of this style
		/// </summary>
		protected int m_ID;
		/// <summary>
		/// The name of this style
		/// </summary>
		protected string m_Name;
		/// <summary>
		/// The name of the spec needed for this style "None" for no requirement
		/// </summary>
		protected string m_SpecKeyName;
		/// <summary>
		/// The level of specialisation required to gain this style
		/// </summary>
		protected int m_SpecLevelRequirement;
		/// <summary>
		/// The icon for this style
		/// </summary>
		protected int m_Icon;
		/// <summary>
		/// The fatique cost for this style in % of total fatique
		/// Will be modified by weapon speed and Realm Abilities
		///		>=5(low), >=10(medium), >=15(high)
		/// </summary>
		protected int m_EnduranceCost;
		/// <summary>
		/// Style requires stealth
		/// </summary>
		protected bool m_StealthRequirement;
		/// <summary>
		/// The opening requirement of this style
		///		0 = offensive opening eg. style in reply to your previous action
		///		1 = defensive opening eg. style in reply to enemy previous action
		///		2 = positional opening eg. front, side, or back
		/// </summary>
		protected int m_openingRequirementType;
		/// <summary>
		/// opening requirement value
		/// for offensive openings the styleid of the required style before this
		/// for defensive openings the styleid of the enemy style required before this
		/// for positional openings:
		///		0 = back of enemy
		///		1 = side of enemy
		///		2 = front of enemy
		/// </summary>
		protected int m_openingRequirementValue; //style required before this one
		/// <summary>
		/// The required result of the previous attack.
		/// For offensive styles the attack result of your last attack
		/// For defensive styles the attack result of your enemies last attack
		///		0 = any
		///		1 = miss
		///		2 = hit
		///		3 = parry
		///		4 = block
		///		5 = evade
		///		6 = fumble
		///		7 = style
		/// </summary>
		protected int m_AttackResultRequirement;
		/// <summary>
		/// This holds the type of weapon needed for this style
		/// See "eObjectType" for values
		/// </summary>
		protected int m_WeaponTypeRequirement;
		//		/// <summary>
		//		/// The damage addition factor of this style
		//		/// </summary>
		//		protected int			m_Damage;
		//		/// <summary>
		//		/// Addition to damage for each level above requirement
		//		/// </summary>
		//		protected int			m_DamageAddPerLevel;
		/// <summary>
		/// GrowthRate as used in Wyrd's spreadsheet
		/// </summary>
		protected double m_growthRate;
		/// <summary>
		/// The bonus to hit value for this style
		/// below 0 = penalty
		/// above 0 = bonus
		/// </summary>
		protected int m_BonusToHit;
		/// <summary>
		/// The bonus to defense for this style
		/// below 0 = penalty
		/// above 0 = bonus
		/// </summary>
		protected int m_BonusToDefense;
		/// <summary>
		/// The animation ID for 2h weapon styles
		/// </summary>
		protected int m_TwoHandAnimation;

		/// <summary>
		/// Randomly cast a proc
		/// </summary>
		protected bool m_RandomProc;
		private int m_classId;

		static bool m_autoSave;

		/// <summary>
		/// The Constructor
		/// </summary>
		public DBStyle()
		{
			m_autoSave = false;
		}

		/// <summary>
		/// The Style ID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public virtual int ID
		{
			get { return m_ID; }
			set { m_ID = value; }
		}

		/// <summary>
		/// The Style Name
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get { return m_Name; }
			set { m_Name = value; Dirty = true; }
		}

		/// <summary>
		/// The Style SpecKeyName
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public string SpecKeyName
		{
			get { return m_SpecKeyName; }
			set { m_SpecKeyName = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Spec Level Requirement
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int SpecLevelRequirement
		{
			get { return m_SpecLevelRequirement; }
			set { m_SpecLevelRequirement = value; Dirty = true; }
		}

		[DataElement(AllowDbNull = false)]
		public int Icon
		{
			get { return m_Icon; }
			set { m_Icon = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Endurance Cost
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int EnduranceCost
		{
			get { return m_EnduranceCost; }
			set { m_EnduranceCost = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Stealth Requirement
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool StealthRequirement
		{
			get { return m_StealthRequirement; }
			set { m_StealthRequirement = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Opening Requirement Type
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int OpeningRequirementType
		{
			get { return m_openingRequirementType; }
			set { m_openingRequirementType = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Opening Requirement Value
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int OpeningRequirementValue
		{
			get { return m_openingRequirementValue; }
			set { m_openingRequirementValue = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Attack Result Requirement
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int AttackResultRequirement
		{
			get { return m_AttackResultRequirement; }
			set { m_AttackResultRequirement = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Weapon Type Requirement
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int WeaponTypeRequirement
		{
			get { return m_WeaponTypeRequirement; }
			set { m_WeaponTypeRequirement = value; Dirty = true; }
		}

		//		[DataElement(AllowDbNull=true)]
		//	  public int Damage
		//	  {
		//	    get { return m_Damage; }
		//	    set { m_Damage = value; Dirty = true; }
		//	  }
		//
		//	[DataElement(AllowDbNull=true)]
		//	  public int DamageAddPerLevel
		//	  {
		//	    get { return m_DamageAddPerLevel; }
		//	    set { m_DamageAddPerLevel = value; Dirty = true; }
		//	  }

		/// <summary>
		/// The Style Growth Rate
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public double GrowthRate
		{
			get { return m_growthRate; }
			set { m_growthRate = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Bonus To Hit
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int BonusToHit
		{
			get { return m_BonusToHit; }
			set { m_BonusToHit = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Bonus to Defense
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int BonusToDefense
		{
			get { return m_BonusToDefense; }
			set { m_BonusToDefense = value; Dirty = true; }
		}

		/// <summary>
		/// The Style Two Hand Animation
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int TwoHandAnimation
		{
			get { return m_TwoHandAnimation; }
			set { m_TwoHandAnimation = value; Dirty = true; }
		}

		/// <summary>
		///(procs) The Style should Randomly cast a proc 
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool RandomProc
		{
			get { return m_RandomProc; }
			set { m_RandomProc = value; Dirty = true; }
		}

		[DataElement(AllowDbNull = false)]
		public int ClassId
		{
			get { return m_classId; }
			set { m_classId = value; Dirty = true; }
		}

		/// <summary>
		/// Autosave in table?
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
