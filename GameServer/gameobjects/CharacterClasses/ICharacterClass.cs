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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Realm;

namespace DOL.GS
{
	/// <summary>
	/// Represents a DOL Character class
	/// </summary>
	public interface ICharacterClass
	{
		/// <summary>
		/// Unique Class Identifier based on eCharacterClass
		/// </summary>
		int ID { get; }

		/// <summary>
		/// ClassName (Used as Translation Key / Title Translation Key)
		/// </summary>
		string Name { get; }
		/// <summary>
		/// Class Female Name
		/// </summary>
		string FemaleName { get; }

		/// <summary>
		/// Base ClassName if available
		/// </summary>
		string BaseName { get; }

		/// <summary>
		/// Class Profession
		/// </summary>
		string Profession { get; }

		/// <summary>
		/// Class Base Hit Points
		/// </summary>
		int BaseHP { get; }

		/// <summary>
		/// Class Spec Points Multiplier
		/// </summary>
		int SpecPointsMultiplier { get; }

		/// <summary>
		/// This is specifically used for adjusting spec points as needed for new training window
		/// For standard DOL classes this will simply return the standard spec multiplier
		/// </summary>
		int AdjustedSpecPointsMultiplier { get; }

		/// <summary>
		/// Class Primary Raising Stat
		/// </summary>
		eStat PrimaryStat { get; }

		/// <summary>
		/// Class Secondary Raising Stat
		/// </summary>
		eStat SecondaryStat { get; }
		
		/// <summary>
		/// Class Tertiary Raising Stat
		/// </summary>
		eStat TertiaryStat { get; }
		
		/// <summary>
		/// Class Mana Stat Used for Spell
		/// </summary>
		eStat ManaStat { get; }
		
		/// <summary>
		/// Class Weapon Skill Base
		/// </summary>
		int WeaponSkillBase { get; }

		/// <summary>
		/// Class Type
		/// </summary>
		eClassType ClassType { get; }

		/// <summary>
		/// Instance Attached GamePlayer
		/// </summary>
		GamePlayer Player { get; }

		/// <summary>
		/// The maximum number of pulsing spells the class can have active simultaneously
		/// </summary>
		ushort MaxPulsingSpells { get; }

		/// <summary>
		/// Wether this class can use Left Handed Weapon
		/// </summary>
		bool CanUseLefthandedWeapon { get; }

		public DefaultClassBehavior Behavior { get; }

		string GetTitle(GamePlayer player, int level);
		IList<string> GetAutotrainableSkills();
		bool HasAdvancedFromBaseClass();
		GameTrainer.eChampionTrainerType ChampionTrainerType();
		List<PlayerRace> EligibleRaces { get; }

		void Init(GamePlayer player);
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
