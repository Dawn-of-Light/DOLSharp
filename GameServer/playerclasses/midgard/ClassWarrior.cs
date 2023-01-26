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
using System.Collections.Generic;
using DOL.GS.Realm;

namespace DOL.GS.PlayerClass
{
	[CharacterClass((int)eCharacterClass.Warrior, "Warrior", "Viking")]
	public class ClassWarrior : ClassViking
	{
		private static readonly List<string> DefaultAutoTrainableSkills = new() { Specs.Axe, Specs.Hammer, Specs.Sword };
		private static readonly List<PlayerRace> DefaultEligibleRaces = new()
		{
			 PlayerRace.Dwarf, PlayerRace.Kobold, PlayerRace.Deifrang, PlayerRace.Norseman, PlayerRace.Troll, PlayerRace.Valkyn,
		};

		public ClassWarrior()
			: base()
		{
			m_profession = "PlayerClass.Profession.HouseofTyr";
			m_specializationMultiplier = 20;
			m_primaryStat = eStat.STR;
			m_secondaryStat = eStat.CON;
			m_tertiaryStat = eStat.DEX;
			m_baseWeaponSkill = 460;
			m_autotrainableSkills = DefaultAutoTrainableSkills;
			m_eligibleRaces = DefaultEligibleRaces;
		}

		public override bool HasAdvancedFromBaseClass()
		{
			return true;
		}
	}
}
