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
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// Represents a DOL Character class
	/// </summary>
	public interface ICharacterClass
	{
		int ID
		{
			get;
		}

		string Name
		{
			get;
		}

		string BaseName
		{
			get;
		}

		string Profession
		{
			get;
		}

		int BaseHP
		{
			get;
		}

		int SpecPointsMultiplier
		{
			get;
		}

		/// <summary>
		/// This is specifically used for adjusting spec points as needed for new training window
		/// For standard DOL classes this will simply return the standard spec multiplier
		/// </summary>
		int AdjustedSpecPointsMultiplier
		{
			get;
		}

		eStat PrimaryStat
		{
			get;
		}

		eStat SecondaryStat
		{
			get;
		}
		eStat TertiaryStat
		{
			get;
		}
		eStat ManaStat
		{
			get;
		}
		int WeaponSkillBase
		{
			get;
		}
		int WeaponSkillRangedBase
		{
			get;
		}

		eClassType ClassType
		{
			get;
		}

		GamePlayer Player
		{
			get;
			set;
		}

		/// <summary>
		/// The maximum number of pulsing spells the class can have active simultaneously
		/// </summary>
		ushort MaxPulsingSpells
		{
			get;
		}

		string GetTitle(int level);
		void OnLevelUp(GamePlayer player);
		void OnRealmLevelUp(GamePlayer player);
		void OnSkillTrained(GamePlayer player, Specialization skill);
		bool CanUseLefthandedWeapon(GamePlayer player);
		IList<string> GetAutotrainableSkills();
		string FemaleName
		{
			get;
		}
		void SwitchToFemaleName();
		bool HasAdvancedFromBaseClass();

		void Init(GamePlayer player);

		void SetControlledBrain(IControlledBrain controlledBrain);
		void CommandNpcRelease();
		void OnPetReleased();
		bool StartAttack(GameObject attackTarget);
		byte HealthPercentGroupWindow { get; }
		ShadeEffect CreateShadeEffect();
		void Shade(bool state);
		bool RemoveFromWorld();
		void Die(GameObject killer);
		void Notify(DOLEvent e, object sender, EventArgs args);
		bool CanChangeCastingSpeed(SpellLine line, Spell spell);
	}

	/// <summary>
	/// The type of character class
	/// </summary>
	public enum eClassType : int
	{
		/// <summary>
		/// The class has access to all spells
		/// </summary>
		ListCaster,
		/// <summary>
		/// The class has access to best one or two spells
		/// </summary>
		Hybrid,
		/// <summary>
		/// The class has no spells
		/// </summary>
		PureTank,
	}
}
