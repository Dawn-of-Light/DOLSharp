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
using System.Collections.Generic;
using DOL.Database.Attributes;
using DOL.GS;
using DOL.Database;

namespace DOL.GS.Styles
{
	/// <summary>
	/// Holds all information needed for a Style in game.
	/// Mainly this class contains the DBStyle class but
	/// it converts some int values into the appropriate enums
	/// </summary>
	public class Style : Skill
	{
		/// <summary>
		/// The opening type of a style
		/// </summary>
		public enum eOpening : int
		{
			/// <summary>
			/// Offensive opening, depending on the attacker's actions
			/// </summary>
			Offensive = 0,
			/// <summary>
			/// Defensive opening, depending on the enemy's action
			/// </summary>
			Defensive = 1,
			/// <summary>
			/// Positional opening, depending on the attacker to target position
			/// </summary>
			Positional = 2
		}

		/// <summary>
		/// The opening positions if the style is a position based style
		/// </summary>
		public enum eOpeningPosition : int
		{
			/// <summary>
			/// Towards back of the target
			/// </summary>
			Back = 0,
			/// <summary>
			/// Towards the side of the target
			/// </summary>
			Side = 1,
			/// <summary>
			/// Towards the front of the target
			/// </summary>
			Front = 2
		}

		/// <summary>
		/// The required attack result of the style 
		/// </summary>
		public enum eAttackResult : int
		{
			/// <summary>
			/// Any attack result is fine
			/// </summary>
			Any = 0,
			/// <summary>
			/// A miss is required
			/// </summary>
			Miss = 1,
			/// <summary>
			/// A hit is required
			/// </summary>
			Hit = 2,
			/// <summary>
			/// A parry is required
			/// </summary>
			Parry = 3,
			/// <summary>
			/// A block is required
			/// </summary>
			Block = 4,
			/// <summary>
			/// An evade is required
			/// </summary>
			Evade = 5,
			/// <summary>
			/// A fumble is required
			/// </summary>
			Fumble = 6,
			/// <summary>
			/// A style is required
			/// </summary>
			Style = 7
		}

		/// <summary>
		/// Special weapon type requirements
		/// </summary>
		public abstract class SpecialWeaponType
		{
			/// <summary>
			/// Both hands should be holding weapons to use style.
			/// Shield is not a weapon in this case.
			/// </summary>
			public const int DualWield = 1000;
			/// <summary>
			/// Stlye can be used with 1h, 2h, dw.
			/// Used for Critical Strike line.
			/// </summary>
			public const int AnyWeapon = 1001;
		}

		/// <summary>
		/// The database style object, used to retrieve information for this object
		/// </summary>
		protected DBStyle baseStyle = null;

		/// <summary>
		/// (readonly) The list of procs available for this style
		/// </summary>
		protected List<DBStyleXSpell> m_Procs;

		/// <summary>
		/// Constructs a new Style object based on a database Style object
		/// </summary>
		/// <param name="style">The database style object this object is based on</param>
		public Style(DBStyle style)
			: base(style.Name, (ushort)style.ID, style.SpecLevelRequirement)
		{
			baseStyle = style;
			m_Procs = new List<DBStyleXSpell>();
		}

        public int ClassID
        {
            get { return baseStyle.ClassId; }
        }

		/// <summary>
		/// (readonly)(procs) The list of procs available for this style
		/// </summary>
		public List<DBStyleXSpell> Procs
		{
			get { return m_Procs; }
		}

		/// <summary>
		/// (readonly) The Specialization's name required to execute this style
		/// </summary>
		public string Spec
		{
			get { return baseStyle.SpecKeyName; }
		}

		/// <summary>
		/// (readonly) The Specialization's level required to execute this style
		/// </summary>
		public int SpecLevelRequirement
		{
			get { return baseStyle.SpecLevelRequirement; }
		}

		/// <summary>
		/// (readonly) The icon of this style
		/// </summary>
		public int Icon
		{
			get { return baseStyle.Icon; }
		}

		/// <summary>
		/// (readonly) The fatique cost of this style in % of player's total fatique
		/// This cost will be modified by weapon speed, realm abilities and magic effects
		/// </summary>
		public int EnduranceCost
		{
			get { return baseStyle.EnduranceCost; }
		}

		/// <summary>
		/// (readonly) Stealth requirement of this style
		/// </summary>
		public bool StealthRequirement
		{
			get { return baseStyle.StealthRequirement; }
		}

		/// <summary>
		/// (readonly) The opening type of this style
		/// </summary>
		public eOpening OpeningRequirementType
		{
			get { return (eOpening)baseStyle.OpeningRequirementType; }
		}

		/// <summary>
		/// (readonly) Depending on the OpeningRequirementType.
		/// If the style is a offensive opened style, this 
		/// holds the style id the attacker is required to
		/// execute before this style. 
		/// If the style is a defensive opened style, this
		/// holds the style id the defender is required to
		/// execute before the attacker can use this style.
		/// (values other than 0 require a nonspecific style)
		/// If the style is a position opened style, this
		/// holds the position requirement.
		/// </summary>
		public int OpeningRequirementValue
		{
			get { return baseStyle.OpeningRequirementValue; }
		}

		/// <summary>
		/// (readonly) The attack result required from 
		/// attacker(offensive style) or defender(defensive style)
		/// </summary>
		public eAttackResult AttackResultRequirement
		{
			get { return (eAttackResult)baseStyle.AttackResultRequirement; }
		}

		/// <summary>
		/// (readonly) The type of weapon required to execute this style.
		/// If not one of SpecialWeaponType then eObjectType is used.
		/// </summary>
		public int WeaponTypeRequirement
		{
			get { return baseStyle.WeaponTypeRequirement; }
		}

		/// <summary>
		/// (readonly) The growth rate of the style
		/// </summary>
		public double GrowthRate
		{
			get { return baseStyle.GrowthRate; }
		}

		/// <summary>
		/// (readonly) The bonus to hit if this style get's executed successfully
		/// </summary>
		public int BonusToHit
		{
			get { return baseStyle.BonusToHit; }
		}

		/// <summary>
		/// (readonly) The bonus to defense if this style get's executed successfully
		/// </summary>
		public int BonusToDefense
		{
			get { return baseStyle.BonusToDefense; }
		}

		/// <summary>
		/// (readonly) The type of this skill, always returns eSkillPage.Styles
		/// </summary>
		public override eSkillPage SkillType
		{
			get { return eSkillPage.Styles; }
		}

		/// <summary>
		/// (readonly) The animation ID for 2h weapon styles
		/// </summary>
		public int TwoHandAnimation
		{
			get { return baseStyle.TwoHandAnimation; }
		}

		/// <summary>
		/// (readonly) (procs) Tell if the proc should be select randomly
		/// </summary>
		public bool RandomProc
		{
			get { return baseStyle.RandomProc; }
		}

		public eArmorSlot ArmorHitLocation
		{
			get { return (eArmorSlot)baseStyle.ArmorHitLocation; }
		}

		/// <summary>
		/// Gets name of required weapon type
		/// </summary>
		/// <returns>name of required weapon type</returns>
		public virtual string GetRequiredWeaponName()
		{
			switch (WeaponTypeRequirement)
			{
				case SpecialWeaponType.DualWield:
					// style line spec name
					Specialization dwSpec = SkillBase.GetSpecialization(Spec);
					if (dwSpec == null) return "unknown DW spec";
					else return dwSpec.Name;

				case SpecialWeaponType.AnyWeapon:
					return "Any";

				default:
					// spec name needed to use weapon type
					string specKeyName = SkillBase.ObjectTypeToSpec((eObjectType)WeaponTypeRequirement);
					if (specKeyName == null)
						return "unknown weapon type";
					Specialization spec = SkillBase.GetSpecialization(specKeyName);
					return spec.Name;
			}
		}


		public override Skill Clone()
		{
			return (Style)MemberwiseClone();
		}
	}
}
