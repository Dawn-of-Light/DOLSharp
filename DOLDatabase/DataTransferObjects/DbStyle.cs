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
using System.Collections;

namespace DOL.Database.DataTransferObjects
{
	/// <summary>
	/// TODO: add neccessary fields for styles
	/// </summary>
	public class DbStyle
	{
		/// <summary>
		/// The ID of this style
		/// </summary>
		protected int			m_id;
		/// <summary>
		/// The name of this style
		/// </summary>
		protected string		m_Name;
		/// <summary>
		/// The name of the spec needed for this style "None" for no requirement
		/// </summary>
		protected string		m_SpecKeyName;
		/// <summary>
		/// The level of specialisation required to gain this style
		/// </summary>
		protected int			m_SpecLevelRequirement;
		/// <summary>
		/// The icon for this style
		/// </summary>
		protected int			m_Icon;
		/// <summary>
		/// The fatique cost for this style in % of total fatique
		/// Will be modified by weapon speed & Realm Abilities
		///		>=5(low), >=10(medium), >=15(high)
		/// </summary>
		protected int			m_EnduranceCost; 
		/// <summary>
		/// Style requires stealth
		/// </summary>
		protected bool			m_StealthRequirement;
		/// <summary>
		/// The opening requirement of this style
		///		0 = offensive opening eg. style in reply to your previous action
		///		1 = defensive opening eg. style in reply to enemy previous action
		///		2 = positional opening eg. front, side, or back
		/// </summary>
		protected int			m_openingRequirementType;
		/// <summary>
		/// opening requirement value
		/// for offensive openings the styleid of the required style before this
		/// for defensive openings the styleid of the enemy style required before this
		/// for positional openings:
		///		0 = back of enemy
		///		1 = side of enemy
		///		2 = front of enemy
		/// </summary>
		protected int			m_openingRequirementValue; //style required before this one
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
		protected int			m_AttackResultRequirement; 
		/// <summary>
		/// This holds the type of weapon needed for this style
		/// See "eObjectType" for values
		/// </summary>
		protected int			m_WeaponTypeRequirement;
		/// <summary>
		/// GrowthRate as used in Wyrd's spreadsheet
		/// </summary>
		protected double		m_growthRate;
		/// <summary>
		/// The type of special style result beside damage
		///		0 = no special
		///		1 = effect
		///		2 = taunt
		/// </summary>
		protected int			m_SpecialType;
		/// <summary>
		/// The value for the special effect of this style
		///		For taunt:
		///			< 0 = detaunt
		///			> 0 = taunt
		///		For effects it is the effectID
		/// </summary>
		protected int			m_SpecialValue;
		/// <summary>
		/// The bonus to hit value for this style
		/// below 0 = penalty
		/// above 0 = bonus
		/// </summary>
		protected int			m_BonusToHit;
		/// <summary>
		/// The bonus to defense for this style
		/// below 0 = penalty
		/// above 0 = bonus
		/// </summary>
		protected int			m_BonusToDefense;
		/// <summary>
		/// The animation ID for 2h weapon styles
		/// </summary>
		protected int			m_TwoHandAnimation;

		

		public int ID
		{
			get 
			{ 
				return m_id; 
			}
			set 
			{
				m_id = value;
			}
		}

		public string Name
		{
			get 
			{ 
				return m_Name;
			}
			set 
			{ 
				m_Name = value; 
			}
		}

		public string SpecKeyName
		{
			get 
			{ 
				return m_SpecKeyName;
			}
			set 
			{ 
				m_SpecKeyName = value;
			}
		}

		public int SpecLevelRequirement
		{
			get
			{ 
				return m_SpecLevelRequirement;
			}
			set
			{ 
				m_SpecLevelRequirement = value; 
			}
		}

		public int EnduranceCost
		{
			get
			{ 
				return m_EnduranceCost;
			}
			set 
			{ 
				m_EnduranceCost = value; 
			}
		}

		public bool StealthRequirement
		{
			get
			{ 
				return m_StealthRequirement;
			}
			set
			{ 
				m_StealthRequirement = value; 
			}
		}

		public int OpeningRequirementType
		{
			get
			{ 
				return m_openingRequirementType; }
			set 
			{
				m_openingRequirementType = value;
			}
		}

	  
		public int OpeningRequirementValue
		{
			get 
			{ 
				return m_openingRequirementValue;
			}
			set 
			{
				m_openingRequirementValue = value;
			}
		}

		
		public int AttackResultRequirement
		{
			get 
			{ 
				return m_AttackResultRequirement;
			}
			set 
			{
				m_AttackResultRequirement = value; 
			}
		}

		public int WeaponTypeRequirement
		{
			get 
			{ 
				return m_WeaponTypeRequirement;
			}
			set 
			{
				m_WeaponTypeRequirement = value; 
			}
		}

		public double GrowthRate
		{
			get
			{ 
				return m_growthRate;
			}
			set
			{
				m_growthRate = value;
			}
		}

		public int SpecialType
		{
			get
			{ 
				return m_SpecialType; 
			}
			set 
			{ 
				m_SpecialType = value; 
			}
		}

		public int SpecialValue
		{
			get
			{ 
				return m_SpecialValue;
			}
			set
			{ 
				m_SpecialValue = value; 
			}
		}

		public int BonusToHit
		{
			get
			{ 
				return m_BonusToHit;
			}
			set 
			{ 
				m_BonusToHit = value;
			}
		}

		
		public int BonusToDefense
		{
			get 
			{ 
				return m_BonusToDefense;
			}
			set 
			{
				m_BonusToDefense = value; 
			}
		}

		
		public int TwoHandAnimation
		{
			get
			{ 
				return m_TwoHandAnimation; 
			}
			set 
			{ 
				m_TwoHandAnimation = value; 
			}
		}
	}
}
